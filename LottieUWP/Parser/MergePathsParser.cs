using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    public static class MergePathsParser
    {
        public static MergePaths Parse(JsonReader reader)
        {
            string name = null;
            MergePaths.MergePathsMode mode = MergePaths.MergePathsMode.Add;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "mm":
                        mode = (MergePaths.MergePathsMode)reader.NextInt();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new MergePaths(name, mode);
        }
    }
}
