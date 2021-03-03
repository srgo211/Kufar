using Leaf.xNet;

namespace _Test
{
    class Program
    {
        static void Main(string[] args)
        {

            string url = "";

            using (var request = new HttpRequest())
            {
                var multipartContent = new MultipartContent()
                {
                    {new StringContent("Harry Potter"), "login"},
                    {new StringContent("Crucio"), "password"},
                    {new FileContent(@"C:\hp.rar"), "file1", "hp.rar"}
                };

                // When response isn't required
                request.Post("https://google.com", multipartContent).None();

                // Or
                var resp = request.Post("https://google.com", multipartContent);
                // And then read as string
                string respStr = resp.ToString();

            }


        }
    }
}
