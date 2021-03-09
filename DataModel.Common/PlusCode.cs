namespace DataModel.Common
{
    public readonly struct PlusCode
    {

        public string Code { get; }
        public int Precision { get; }

        public PlusCode(string code, int precision)
        {
            (Code, Precision) = (code, precision);
        }
    }




}
