using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

public class MemoryUtils {
	public static T StructFromBytes<T>(byte[] buffer) where T : struct {
		var handle = GCHandle.Alloc (buffer, GCHandleType.Pinned);
		T result = (T)Marshal.PtrToStructure (handle.AddrOfPinnedObject(), typeof(T));
		handle.Free ();
		return result;
	}


    public static byte[] StructToBytes<T>(T value) where T : struct {
        byte[] result = new byte[Marshal.SizeOf(typeof(T))];
        var handle = GCHandle.Alloc(result, GCHandleType.Pinned);
        Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
        handle.Free();
        return result;
    }

    public static T ReadStruct<T>(Stream stream) where T : struct {
		BinaryReader reader = new BinaryReader (stream);
		return StructFromBytes<T>(reader.ReadBytes(Marshal.SizeOf(typeof(T))));
	}

    public static void WriteStruct<T>(Stream stream, T value) where T : struct {
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(StructToBytes<T>(value));
    }

    public static string ReadAnsiString(Stream stream){
		byte b = 0;
		List<byte> bytes = new List<byte> ();
		do {
			b = (byte)stream.ReadByte ();
			bytes.Add(b);
		} while(b != 0); 

		return Encoding.ASCII.GetString (bytes.ToArray ());
	}

    public static void WriteAnsiString(string value, Stream stream) {
        BinaryWriter writer = new BinaryWriter(stream);
        if (!string.IsNullOrEmpty(value)) {
            writer.Write(Encoding.ASCII.GetBytes(value));
        }
        writer.Write((byte)0);
    }
}

public static class AMeshEx {
    public static Vector4 ToVector3(this AMesh.AVector4 v) {
        return new Vector4(v.X, v.Y, v.Z, v.W);
    }


    public static Vector3 ToVector3(this AMesh.AVector3 v) {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static Vector2 ToVector2(this AMesh.AVector2 v) {
        return new Vector2(v.X, v.Y);
    }

    public static AMesh.AVector3 ToAVector3(this Vector3 v) {
        return new AMesh.AVector3() {X=v.x, Y=v.y, Z = v.z };
    }

    public static AMesh.AVector4 ToAVector4(this Vector4 v) {
        return new AMesh.AVector4() { X = v.x, Y = v.y, Z = v.z, W = v.w};
    }

    public static AMesh.AVector2 ToAVector2(this Vector2 v) {
        return new AMesh.AVector2() { X = v.x, Y = v.y };
    }
}

public class AMesh {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AVector4 {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public static AVector4 Min(AVector4 a, AVector4 b) {
            return new AVector4() {
                X = Mathf.Min(a.X, b.X),
                Y = Mathf.Min(a.Y, b.Y),
                Z = Mathf.Min(a.Z, b.Z),
                W = Mathf.Min(a.W, b.W)
            };
        }

        public  static AVector4 Max(AVector4 a, AVector4 b) {
            return new AVector4() {
                X = Mathf.Max(a.X, b.X),
                Y = Mathf.Max(a.Y, b.Y),
                Z = Mathf.Max(a.Z, b.Z),
                W = Mathf.Max(a.W, b.W)
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AVector3 {
		public float X;
		public float Y;
		public float Z;

        public static AVector3 Min(AVector3 a, AVector3 b) {
            return new AVector3() {
                X = Mathf.Min(a.X, b.X),
                Y = Mathf.Min(a.Y, b.Y),
                Z = Mathf.Min(a.Z, b.Z)
            };
        }

        public static AVector3 Max(AVector3 a, AVector3 b) {
            return new AVector3() {
                X = Mathf.Max(a.X, b.X),
                Y = Mathf.Max(a.Y, b.Y),
                Z = Mathf.Max(a.Z, b.Z)
            };
        }
    }

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AVector2 {
		public float X;
		public float Y;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ABoundingBox {
		public AVector3 Min;
		public AVector3 Max;
	}

    [Flags]
    public enum AMeshFlags : byte {
        None = 0,
        HaveTangents = 1 << 0
    }

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AMeshHeader{
        public byte Version;
		public byte WeightsCount;
		public byte BonesCount;
		public byte AnimationsCount;
		public ushort PolygonsCount;
		public ushort VerticesCount;
		public ABoundingBox BoundingBox;

        public AMeshFlags Flags;
        public byte Reserved1;
        public ushort Reserved2;
		public uint Reserved3;

		public float AnimationStartTime;
		public float AnimationEndTime;
		public float AnimationDeltaTime;

		public override string ToString ()
		{
			return string.Format ("[AMeshHeader] Version: {0} Vertices: {1} Polygons: {2}", Version, VerticesCount, PolygonsCount);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AVertex{
		public AVector3 Position;
        public AVector2 UV;
        public AVector3 Normal;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AVertexTN {
        public AVector3 Position;
        public AVector2 UV;
        public AVector3 Normal;
        public AVector4 Tangent;
    }

    public AMeshHeader Header;
	public string MaterialName;


    public List<AVector3> Position = new List<AVector3>();
    public List<AVector2> UV0 = new List<AVector2>();
    public List<AVector3> Normal = new List<AVector3>();
    public List<AVector4> Tangent;
    //public AVertex[] Vertices;

    public ushort[] Indices;

    public void SwapFaceOrder() {
        int cnt = Indices.Length;
        for (int i = 0; i < cnt; i+=3) {
            var tmp = Indices[i + 1];
            Indices[i + 1] = Indices[i + 2];
            Indices[i + 2] = tmp;
        }
    }

    public void Save(Stream stream) {
        MemoryUtils.WriteStruct(stream, Header);
        MemoryUtils.WriteAnsiString(MaterialName, stream);

        bool tangents = (Header.Flags & AMeshFlags.HaveTangents) != AMeshFlags.None;

        if (tangents) {
            
            for (ushort i = 0; i < Header.VerticesCount; ++i) {
                MemoryUtils.WriteStruct(stream, Position[i]);
                MemoryUtils.WriteStruct(stream, UV0[i]);
                MemoryUtils.WriteStruct(stream, Normal[i]);
                MemoryUtils.WriteStruct(stream, Tangent[i]);
            }
        } else {
            for (ushort i = 0; i < Header.VerticesCount; ++i) {
                MemoryUtils.WriteStruct(stream, Position[i]);
                MemoryUtils.WriteStruct(stream, Normal[i]);
                MemoryUtils.WriteStruct(stream, UV0[i]);
            }
        }
          

        BinaryWriter writer = new BinaryWriter(stream);
        foreach (var index in Indices) {
            writer.Write(index);
        }
    }

    public static AMesh Load(Stream stream){
		AMesh result = new AMesh ();
		result.Header = MemoryUtils.ReadStruct<AMeshHeader>(stream);
		result.MaterialName = MemoryUtils.ReadAnsiString (stream);
	
        

        bool tangents = (result.Header.Flags & AMeshFlags.HaveTangents) != AMeshFlags.None;
        if (tangents) {
            result.Tangent = new List<AVector4>();

            for (ushort i = 0; i < result.Header.VerticesCount; ++i) {
                result.Position.Add(MemoryUtils.ReadStruct<AVector3>(stream));
                result.UV0.Add(MemoryUtils.ReadStruct<AVector2>(stream));
                result.Normal.Add(MemoryUtils.ReadStruct<AVector3>(stream));
                result.Tangent.Add(MemoryUtils.ReadStruct<AVector4>(stream));
            }
        } else {
            for (ushort i = 0; i < result.Header.VerticesCount; ++i) {
                result.Position.Add(MemoryUtils.ReadStruct<AVector3>(stream));
                result.Normal.Add(MemoryUtils.ReadStruct<AVector3>(stream));
                result.UV0.Add(MemoryUtils.ReadStruct<AVector2>(stream));
            }
        }




        var indices = new List<ushort>();
        BinaryReader reader = new BinaryReader(stream);
        for (ushort i = 0; i < result.Header.PolygonsCount; ++i) {
            for (ushort j = 0; j < 3; ++j) {
                indices.Add(reader.ReadUInt16());
            } 
        }

       
        result.Indices = indices.ToArray();

        return result;
	}


}
