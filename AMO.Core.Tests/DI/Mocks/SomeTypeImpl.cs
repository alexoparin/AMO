namespace DbImport2.Core.Tests.DI.Mocks
{
    internal class SomeTypeImpl : ISomeType
    {
        internal static int CreatedCount { get; private set; }

        public int    Id   { get; set; }
        public string Name { get; set; }


        public SomeTypeImpl()
        {
            ++CreatedCount;
        }

        internal static void Reset()
        {
            CreatedCount = 0;
        }
    }
}
