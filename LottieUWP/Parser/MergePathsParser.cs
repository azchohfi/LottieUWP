using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    static class MergePathsParser
    {
        internal static MergePaths Parse(JsonReader reader)
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
