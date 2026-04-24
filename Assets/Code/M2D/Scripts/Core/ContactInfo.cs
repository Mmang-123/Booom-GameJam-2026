
using UnityEngine;

namespace Mmang.M2D
{
    public struct ContactInfo
    {
        public bool Valid { get; private set; }
        public BoxCollider2D BoxCollider;
    
        public static ContactInfo None => new();

        public static ContactInfo Create(BoxCollider2D boxCollider)
        {
            ContactInfo info = new()
            {
                Valid = true,
                BoxCollider = boxCollider,
            };

            return info;
        }   
    }
}