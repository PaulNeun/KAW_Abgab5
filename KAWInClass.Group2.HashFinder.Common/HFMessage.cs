using System;

namespace KAWInClass.Group2.HashFinder.Common
{
    [Serializable]
    public class HFMessage
    {
        public int Start { get; set; }
        public int End { get; set; }

        public string HashedSecret { get; set; }
    }
}
