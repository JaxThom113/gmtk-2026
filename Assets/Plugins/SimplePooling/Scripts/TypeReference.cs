using System;

namespace Sezylrin.SimplePooling
{
    internal class TypeReference
    {
        public TypeReference() { }
        public TypeReference(Type desiredType)
        {
            this.desiredType = desiredType;
        }
        public Type desiredType;
        // Implicit conversion from SerializableType to Type
        public static implicit operator Type(TypeReference sType) => sType.desiredType;

        // Implicit conversion from Type to SerializableType
        public static implicit operator TypeReference(Type type) => new() { desiredType = type };
    }
}
