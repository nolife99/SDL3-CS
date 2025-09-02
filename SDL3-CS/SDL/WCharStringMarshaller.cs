namespace SDL3;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

[CustomMarshaller(typeof(string), MarshalMode.Default, typeof(WCharStringMarshaller))]
public static class WCharStringMarshaller
{
    // The size in bytes of a wide character for the current runtime
    public static nuint WCharSize => (nuint)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 2 : 4);

    // Выбираем реализацию в зависимости от платформы
    public static nint ConvertToUnmanaged(string? managed)
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            WChar16.ConvertToUnmanaged(managed) :
            WChar32.ConvertToUnmanaged(managed);

    public static string? ConvertToManaged(nint unmanaged)
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            WChar16.ConvertToManaged(unmanaged) :
            WChar32.ConvertToManaged(unmanaged);

    public static void Free(nint ptr) => Marshal.FreeHGlobal(ptr);

    public static class WChar16 // Windows (UTF-16)
    {
        public static nint ConvertToUnmanaged(string? managed)
        {
            if (managed is null) return nint.Zero;

            var bytes = Encoding.Unicode.GetBytes(managed + '\0'); // null-terminated
            var ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            return ptr;
        }

        public static string? ConvertToManaged(nint unmanaged)
            => unmanaged == nint.Zero ? null : Marshal.PtrToStringUni(unmanaged);

        public static void Free(nint ptr) => Marshal.FreeHGlobal(ptr);
    }

    public static class WChar32 // Linux/macOS (UTF-32)
    {
        public static nint ConvertToUnmanaged(string? managed)
        {
            if (managed is null) return nint.Zero;

            var utf32 = Encoding.UTF32.GetBytes(managed + '\0');
            var ptr = Marshal.AllocHGlobal(utf32.Length);
            Marshal.Copy(utf32, 0, ptr, utf32.Length);
            return ptr;
        }

        public static string? ConvertToManaged(nint unmanaged)
            => unmanaged == nint.Zero ? null : PtrToStringUTF32(unmanaged);

        public static string? PtrToStringUTF32(nint ptr)
        {
            if (ptr == nint.Zero) return null;

            List<byte> bytes = [];

            unsafe
            {
                var p = (uint*)ptr;
                while (*p != 0)
                {
                    var utf32Char = BitConverter.GetBytes(*p);
                    bytes.AddRange(utf32Char);
                    p++;
                }
            }

            return Encoding.UTF32.GetString(bytes.ToArray());
        }

        public static void Free(nint ptr) => Marshal.FreeHGlobal(ptr);
    }
}