﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static MiraCore.Client.MessageHeader;

namespace MiraCore.Client.FileExplorer
{
    public static class FileExplorerExtensions
    {
        public enum OpenOnlyFlags
        {
            O_RDONLY = 0,
            O_WRONLY = 1,
            O_RDWR = 2,
            O_ACCMODE = 3
        }

        public const int c_MaxBufferLength = 0x1000;
        public const int c_MaxPathLength = 0x400;
        public const int c_MaxNameLength = 255;

        public static int Open(this MiraConnection p_Connection, string p_Path, int p_Flags, int p_Mode)
        {
            if (p_Connection == null)
                return -1;

            if (string.IsNullOrWhiteSpace(p_Path))
                return -1;

            var s_RequestMessage = new Message(
                MessageCategory.File,
                (uint)FileExplorerCommands.FileExplorer_Open,
                true,
                new FileExplorerOpenRequest(p_Path, p_Flags, p_Mode).Serialize());

            var (s_Response, s_Payload) = p_Connection.SendMessageWithResponse<FileExplorerOpenResponse>(s_RequestMessage);
            if (s_Response == null)
                return -1;

            return s_Response.Error;
        }

        public static void Close(this MiraConnection p_Connection, int p_Handle)
        {
            if (p_Connection == null)
                return;

            if (p_Handle < 0)
                return;

            var s_RequestMessage = new Message(
                MessageCategory.File,
                (uint)FileExplorerCommands.FileExplorer_Close,
                true,
                new FileExplorerCloseRequest
                {
                    Handle = p_Handle
                }.Serialize());

            var (s_Response, s_Payload) = p_Connection.SendMessageWithResponse<FileExplorerCloseResponse>(s_RequestMessage);
            if (s_Response == null)
                return;
        }

        public static byte[] Read(this MiraConnection p_Connection, int p_Handle, ulong p_Offset, int p_Count)
        {
            if (p_Connection == null)
                return null;

            if (p_Handle < 0)
                return null;

            var s_RequestMessage = new Message(
                MessageCategory.File,
                (uint)FileExplorerCommands.FileExplorer_Read,
                true,
                new FileExplorerReadRequest
                {
                    Handle = p_Handle,
                    Count = p_Count
                }.Serialize());

            var (s_Response, s_Payload) = p_Connection.SendMessageWithResponse<FileExplorerReadResponse>(s_RequestMessage);
            if (s_Response == null)
                return null;

            if (s_Response.Error < 0)
                return null;

            if (s_Payload.Data == null)
                return null;

            var s_FinalData = new byte[s_Payload.Count];
            Buffer.BlockCopy(s_Payload.Data, 0, s_FinalData, 0, s_Payload.Count);

            return s_FinalData;
        }

        public static bool Write(this MiraConnection p_Connection, int p_Handle, byte[] p_Data)
        {
            if (p_Connection == null)
                return false;

            if (p_Handle < 0)
                return false;

            var s_RequestMessage = new Message(
                MessageCategory.File,
                (uint)FileExplorerCommands.FileExplorer_Write,
                true,
                new FileExplorerWriteRequest
                {
                    Data = p_Data,
                    Count = p_Data.Length,
                    Handle = p_Handle
                }.Serialize());

            var (s_Response, s_Payload) = p_Connection.SendMessageWithResponse<FileExplorerWriteResponse>(s_RequestMessage);
            if (s_Response == null)
                return false;

            return s_Response.Error >= 0;
        }

        public static List<FileExplorerDent> GetDents(this MiraConnection p_Connection, string p_Path)
        {
            var s_DentList = new List<FileExplorerDent>();

            if (p_Connection == null)
                return s_DentList;

            if (string.IsNullOrWhiteSpace(p_Path))
                return s_DentList;

            var s_Request = new Message(
                MessageCategory.File,
                (uint)FileExplorerCommands.FileExplorer_GetDents,
                true,
                new FileExplorerGetdentsRequest(p_Path).Serialize());

            p_Connection.SendMessage(s_Request);

            do
            {
                var (s_Response, s_Payload) = p_Connection.RecvMessage<FileExplorerGetdentsResponse>(s_Request);
                if (s_Response == null || s_Response?.Error < 0 || s_Payload == null)
                    return s_DentList;

                var s_DentIndex = s_Payload.DentIndex;
                if (s_DentIndex == ulong.MaxValue)
                    break;

                if (new string(s_Payload.Dent.Name) == "." ||
                    new string(s_Payload.Dent.Name) == "..")
                    continue;

                s_DentList.Add(s_Payload.Dent);
            }
            while (true);
            
            return s_DentList;
        }

        public static FileExplorerStat Stat(this MiraConnection p_Connection, string p_Path)
        {
            if (p_Connection == null)
                return null;

            if (string.IsNullOrWhiteSpace(p_Path))
                return null;

            var s_Handle = p_Connection.Open(p_Path, (int)OpenOnlyFlags.O_RDONLY, 0777);
            if (s_Handle < 0)
                return null;

            var s_Stat = p_Connection.Stat(s_Handle);

            p_Connection.Close(s_Handle);

            return s_Stat;
        }

        public static FileExplorerStat Stat(this MiraConnection p_Connection, int p_Handle)
        {
            if (p_Connection == null)
                return null;

            if (p_Handle < 0)
                return null;

            var s_Request = new Message(
                MessageCategory.File,
                (uint)FileExplorerCommands.FileExplorer_Stat,
                true,
                new FileExplorerStatRequest(p_Handle).Serialize());

            var (s_Response, s_Payload) = p_Connection.SendMessageWithResponse<FileExplorerStat>(s_Request);
            if (s_Response == null)
                return null;

            if (s_Response.Error < 0)
                return null;

            return s_Payload;
        }

        public static bool Echo(this MiraConnection p_Connection, string p_Message)
        {
            if (p_Connection == null)
                return false;

            if (string.IsNullOrWhiteSpace(p_Message))
                return false;

            if (p_Message.Length > ushort.MaxValue)
                return false;

            var s_Message = new Message(
                MessageCategory.File, 
                (uint)FileExplorerCommands.FileExplorer_Echo, 
                true,
                new FileExplorerEcho(p_Message).Serialize());

            return p_Connection.SendMessage(s_Message);
        }

        public static FileStream DownloadFile(this MiraConnection p_Connection, string p_Path, string p_OutputPath, Action<int, bool> p_StatusCallback = null)
        {
            var s_Stream = File.OpenWrite(p_OutputPath);

            var s_FileHandle = Open(p_Connection, p_Path, (int)OpenOnlyFlags.O_RDONLY, 0);
            if (s_FileHandle < 0)
            {
                p_StatusCallback?.Invoke(0, true);
                return null;
            }

            var s_Stat = Stat(p_Connection, s_FileHandle);
            if (s_Stat == null)
            {
                p_StatusCallback?.Invoke(0, true);
                return null;
            }

            var s_ChunkSize = 0x1000;
            var s_Chunks = s_Stat.Size / s_ChunkSize;
            var s_Leftover = (int)s_Stat.Size % s_ChunkSize;

            ulong s_Offset = 0;

            using (var s_Writer = new BinaryWriter(s_Stream, Encoding.ASCII, true))
            {
                for (var i = 0; i < s_Chunks; ++i)
                {
                    var l_Data = Read(p_Connection, s_FileHandle, s_Offset, s_ChunkSize);
                    if (l_Data == null)
                    {
                        p_StatusCallback?.Invoke(0, true);
                        return null;
                    }

                    // Write a chunk
                    s_Writer.Write(l_Data);

                    // Increment our offset
                    s_Offset += (ulong)l_Data.LongLength;

                    // Calculate and update status
                    p_StatusCallback?.Invoke((int)(((float)s_Offset / (float)s_Stat.Size) * 100), false);
                }

                // Write the leftover data
                var s_Data = Read(p_Connection, s_FileHandle, s_Offset, s_Leftover);
                if (s_Data == null)
                {
                    p_StatusCallback?.Invoke(0, true);
                    return null;
                }

                // Write the leftover
                s_Writer.Write(s_Data);

                // Increment our offset
                s_Offset += (ulong)s_Data.LongLength;

                // Calculate and update status
                p_StatusCallback?.Invoke((int)(((float)s_Offset / (float)s_Stat.Size) * 100), false);
            }

            return s_Stream;
        }

        public static byte[] DecryptSelf(this MiraConnection p_Connection, string p_Path)
        {
            if(p_Connection == null)
                return null;

            if (p_Path.Length > FileExplorerExtensions.c_MaxPathLength)
                return null;

            var s_Request = new Message(
                MessageCategory.File,
                (uint)FileExplorerCommands.FileExplorer_DecryptSelf,
                true,
                new FileExplorerDecryptSelfRequest(p_Path).Serialize());

            p_Connection.SendMessage(s_Request);

            var s_ResponseList = new List<FileExplorerDecryptSelfResponse>();
            do
            {
                var (s_Response, s_Payload) = p_Connection.RecvMessage<FileExplorerDecryptSelfResponse>(s_Request);
                if (s_Response == null || s_Response?.Error < 0 || s_Payload == null)
                    return null;

                var s_ChunkIndex = s_Payload.Index;
                if (s_ChunkIndex == ulong.MaxValue)
                    break;
                
                s_ResponseList.Add(s_Payload);
            }
            while (true);

            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                for (var i = 0; i < s_ResponseList.Count; ++i)
                {
                    s_Writer.Seek((int)s_ResponseList[i].Offset, SeekOrigin.Begin);
                    s_Writer.Write(s_ResponseList[i].Data);
                }

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }

        public static byte[] DownloadFile(this MiraConnection p_Connection, string p_Path, Action<int, bool> p_StatusCallback = null)
        {
            var s_FileHandle = Open(p_Connection, p_Path, (int)OpenOnlyFlags.O_RDONLY, 0);
            if (s_FileHandle < 0)
            {
                p_StatusCallback?.Invoke(0, true);
                return null;
            }

            var s_Stat = Stat(p_Connection, s_FileHandle);
            if (s_Stat == null)
            {
                p_StatusCallback?.Invoke(0, true);
                return null;
            }

            var s_ChunkSize = 0x1000;
            var s_Chunks = s_Stat.Size / s_ChunkSize;
            var s_Leftover = (int)s_Stat.Size % s_ChunkSize;

            ulong s_Offset = 0;

            byte[] s_ReturnData = null;
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                for (var i = 0; i < s_Chunks; ++i)
                {
                    var l_Data = Read(p_Connection, s_FileHandle, s_Offset, s_ChunkSize);
                    if (l_Data == null)
                    {
                        p_StatusCallback?.Invoke(0, true);
                        return null;
                    }

                    // Write a chunk
                    s_Writer.Write(l_Data);

                    // Increment our offset
                    s_Offset += (ulong)l_Data.LongLength;

                    // Calculate and update status
                    p_StatusCallback?.Invoke((int)(((float)s_Offset / (float)s_Stat.Size) * 100), false);
                }

                // Write the leftover data
                var s_Data = Read(p_Connection, s_FileHandle, s_Offset, s_Leftover);
                if (s_Data == null)
                {
                    p_StatusCallback?.Invoke(0, true);
                    return null;
                }

                // Write the leftover
                s_Writer.Write(s_Data);

                // Increment our offset
                s_Offset += (ulong)s_Data.LongLength;

                // Calculate and update status
                p_StatusCallback?.Invoke((int)(((float)s_Offset / (float)s_Stat.Size) * 100), false);

                s_ReturnData = ((MemoryStream)s_Writer.BaseStream).ToArray();
            }

            return s_ReturnData;
        }

        public static bool Unlink(this MiraConnection p_Connection, string p_Path)
        {
            if (p_Connection == null)
                return false;

            if (string.IsNullOrWhiteSpace(p_Path))
                return false;

            if (p_Path.Length > c_MaxPathLength)
                return false;

            var s_Message = new Message(
                MessageCategory.File,
                (uint)FileExplorerCommands.FileExplorer_Unlink,
                true,
                new FileExplorerUnlinkRequest(p_Path).Serialize());

            return p_Connection.SendMessage(s_Message);
        }
    }
}