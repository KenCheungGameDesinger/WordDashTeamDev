using System.Collections;
using Unity.Netcode.Components;
using UnityEngine;

namespace Assets
{
    public class OwnerNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}