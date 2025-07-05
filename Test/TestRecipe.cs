namespace RecipaediaEX.Test {
    public class TestRecipe : IRecipe {
        public string Description => "This is a test recipe";
        public string Message => "This is a message of a test recipe";
        public int DisplayOrder => 0;

        public string Value = string.Empty;
        public int Attribute = 0;

        public bool Match(IRecipe actual) => true;
    }
}