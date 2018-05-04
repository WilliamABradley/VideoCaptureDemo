using ObjCRuntime;

namespace VideoDemo.Services
{
    public static class AppService
    {
        public static bool OnDevice = Runtime.Arch == Arch.DEVICE;
    }
}