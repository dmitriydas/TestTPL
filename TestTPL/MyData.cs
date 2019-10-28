namespace TestTPL
{
    public class MyData1
    {
        public int Id { get; set; }

        public MyData1(int id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
