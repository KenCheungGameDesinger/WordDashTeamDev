﻿using System.Collections;
using Unity.Netcode.Components;
using UnityEngine;

namespace Assets
{
    
    public class OwnerNetworkTransform : NetworkTransform
    {
        /// <summary>
        /// Used to determine who can write to this transform. Owner client only.
        /// This imposes state to the server. This is putting trust on your clients. Make sure no security-sensitive features use this transform.
        /// </summary>
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}