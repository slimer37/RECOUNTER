using System;
using UnityEngine;

namespace Recounter
{
    public enum GetComponentType { InParent, Self, InChildren }

    public static class GetComponentTypeExtensions
    {
        public static Func<Transform, T> GetMethod<T>(this GetComponentType getComponentType) where T : class =>
            getComponentType switch
            {
                GetComponentType.InParent => t => t.GetComponentInParent<T>(),
                GetComponentType.Self => t => t.GetComponent<T>(),
                GetComponentType.InChildren => t => t.GetComponentInChildren<T>(),
                _ => throw new ArgumentOutOfRangeException(nameof(getComponentType))
            };
    }
}