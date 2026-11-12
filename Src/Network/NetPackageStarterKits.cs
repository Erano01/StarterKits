namespace StarterKits.Network
{
    // The game registers every NetPackage subclass found in loaded mod assemblies automatically
    // (NetPackageManager -> ReflectionHelpers.FindTypesImplementingBase). Both packages only flow
    // server -> client; a client cannot push kits to a server.

    /// <summary>The server's kit list (kits.xml as text).</summary>
    public class NetPackageStarterKitsData : NetPackage
    {
        private string xml = string.Empty;

        public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

        public override bool Compress => true;

        public NetPackageStarterKitsData Setup(string _xml)
        {
            xml = _xml ?? string.Empty;
            return this;
        }

        public override void read(PooledBinaryReader _reader)
        {
            xml = _reader.ReadString();
        }

        public override void write(PooledBinaryWriter _writer)
        {
            base.write(_writer);

            // Cast: PooledBinaryWriter also has Span overloads that net48 reference assemblies cannot resolve.
            var writer = (System.IO.BinaryWriter)_writer;
            writer.Write(xml);
        }

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            if (!KitSync.IsRemoteClient)
            {
                return;
            }

            KitSync.OnKitsReceived(xml);
        }
    }

    /// <summary>One chunk of a kit picture (Kits/Images/&lt;name&gt;.png on the server).</summary>
    public class NetPackageStarterKitsImage : NetPackage
    {
        private string name = string.Empty;
        private int index;
        private int count;
        private byte[] data = new byte[0];

        public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

        public NetPackageStarterKitsImage Setup(string _name, int _index, int _count, byte[] _data)
        {
            name = _name ?? string.Empty;
            index = _index;
            count = _count;
            data = _data ?? new byte[0];
            return this;
        }

        public override void read(PooledBinaryReader _reader)
        {
            name = _reader.ReadString();
            index = _reader.ReadInt32();
            count = _reader.ReadInt32();
            int length = _reader.ReadInt32();
            if (length < 0 || length > KitSync.ChunkSize)
            {
                data = null;
                return;
            }

            data = _reader.ReadBytes(length);
        }

        public override void write(PooledBinaryWriter _writer)
        {
            base.write(_writer);

            var writer = (System.IO.BinaryWriter)_writer;
            writer.Write(name);
            writer.Write(index);
            writer.Write(count);
            writer.Write(data.Length);
            writer.Write(data);
        }

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            if (!KitSync.IsRemoteClient)
            {
                return;
            }

            KitSync.OnImageChunk(name, index, count, data);
        }
    }
}
