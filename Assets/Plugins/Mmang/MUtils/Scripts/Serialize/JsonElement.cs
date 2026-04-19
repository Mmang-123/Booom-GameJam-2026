using System;

namespace Mmang.Util
{
    [Serializable]
    public struct JsonElement
    {
        public string type;
        public string jsonDatas;

        public override string ToString()
        {
            return "type: " + type + " | JSON: " + jsonDatas;
        }
    }
}