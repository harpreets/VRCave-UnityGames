using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Will persist the state of the infinite object generator as well as the infintie object history.
 * This class is used when you want to perserve the starting state of a game, for example showing a tutorial
 * Do not directly add this class to any game object. The Infinite Object Persistence Editor will do that for you.
 */
public class InfiniteObjectPersistence : MonoBehaviour {

    // From Infinite Object Generator:
    public float[] localDistance;
    public float[] localSceneDistance;
    public float[] localPlatformHeight;
    public float[] localSceneHeight;

    // From Infinite Object History:
    public int[] objectSpawnIndex;
    public int[] objectTypeSpawnIndex;
    public int[] lastLocalIndex;
    public float[] lastObjectSpawnDistance;
    public float[] latestObjectTypeSpawnDistance;
    public float[] totalDistance;
    public float[] totalSceneDistance;

}
