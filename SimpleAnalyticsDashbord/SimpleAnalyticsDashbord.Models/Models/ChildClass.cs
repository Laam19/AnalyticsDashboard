namespace SimpleAnalyticsDashbord.Models
{
    public class ChildClass
    {
        public string Device { get; set; }
        public int Value { get; set; }

        public ChildClass(string device, int value)
        {
            Device = device;
            Value = value;
        }

    }
}