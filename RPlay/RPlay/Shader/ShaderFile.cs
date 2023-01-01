namespace RPlay
{
    public static class ShaderFile
    {
        private static string Path => Application.Path + "Shaders/";

        public static string GetShader(string name)
        {
            string text = File.ReadAllText(Path + name);
            string[] lines = text.Split("\n");

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line.Contains("#include"))
                {
                    string[] includes = line.Split('"');
                    var filename = includes[1];

                    lines[i] = GetShader(filename);
                }
            }

            string rebuilded = "";

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                rebuilded += line + "\n";
            }
            
            return rebuilded;
        }
    }
}