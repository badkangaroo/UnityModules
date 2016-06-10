﻿using UnityEngine;
using System.Collections.Generic;
using LeapInternal;
using Leap.Unity.Interaction.CApi;

namespace Leap.Unity.Interaction {

  public class HoldingPoseControllerKabsch : IHoldingPoseController {
    public const int NUM_FINGERS = 5;
    public const int NUM_BONES = 4;

    protected Dictionary<int, HandPointCollection> _handIdToPoints;
    protected LEAP_IE_KABSCH _kabsch;

    protected override void Init(InteractionBehaviour obj) {
      base.Init(obj);

      _handIdToPoints = new Dictionary<int, HandPointCollection>();
      KabschC.Construct(ref _kabsch);
    }

    public override void TransferHandId(int oldId, int newId) {
      _handIdToPoints[newId] = _handIdToPoints[oldId];
      _handIdToPoints.Remove(oldId);
    }

    public override void AddHand(Hand hand) {
      var newCollection = HandPointCollection.Create(_obj.warper);
      _handIdToPoints[hand.Id] = newCollection;

      for (int f = 0; f < NUM_FINGERS; f++) {
        Finger finger = hand.Fingers[f];
        Finger.FingerType fingerType = finger.Type;

        for (int j = 0; j < NUM_BONES; j++) {
          Bone.BoneType boneType = (Bone.BoneType)j;
          Bone bone = finger.Bone(boneType);

          Vector3 bonePos = bone.NextJoint.ToVector3();

          //Global position of the point is just the position of the joint itself
          newCollection.SetGlobalPosition(bonePos, fingerType, boneType);
        }
      }
    }

    public override void RemoveHand(Hand hand) {
      var collection = _handIdToPoints[hand.Id];
      _handIdToPoints.Remove(hand.Id);

      //Return the collection to the pool so it can be re-used
      HandPointCollection.Return(collection);
    }

    public override void GetHoldingPose(ReadonlyList<Hand> hands, out Vector3 newPosition, out Quaternion newRotation) {
      KabschC.Reset(ref _kabsch);

      for (int h = 0; h < hands.Count; h++) {
        Hand hand = hands[h];

        var collection = _handIdToPoints[hand.Id];

        for (int f = 0; f < NUM_FINGERS; f++) {
          Finger finger = hand.Fingers[f];
          Finger.FingerType fingerType = finger.Type;

          for (int j = 0; j < NUM_BONES; j++) {
            Bone.BoneType boneType = (Bone.BoneType)j;
            Bone bone = finger.Bone(boneType);

            Vector3 localPos = collection.GetLocalPosition(fingerType, boneType);
            Vector3 bonePos = bone.NextJoint.ToVector3();

            //Do the solve such that the objects positions are matched to the new bone positions
            LEAP_VECTOR point1 = localPos.ToCVector();
            LEAP_VECTOR point2 = bonePos.ToCVector();

            KabschC.AddPoint(ref _kabsch, ref point1, ref point2, 1.0f);
          }
        }
      }

      performSolve();

      LEAP_VECTOR leapTranslation;
      LEAP_QUATERNION leapRotation;
      KabschC.GetTranslation(ref _kabsch, out leapTranslation);
      KabschC.GetRotation(ref _kabsch, out leapRotation);

      newPosition = leapTranslation.ToVector3();
      newRotation = leapRotation.ToQuaternion();
    }

    protected void performSolve() {
      KabschC.Solve(ref _kabsch);
    }

    protected class HandPointCollection {
      //Without a pool, you might end up with 2 instances per object
      //With a pool, likely there will only ever be 2 instances!
      private static Stack<HandPointCollection> _handPointCollectionPool = new Stack<HandPointCollection>();

      private Vector3[] _localPositions;
      private Matrix4x4 _inverseTransformMatrix;

      public static HandPointCollection Create(RigidbodyWarper warper) {
        HandPointCollection collection;
        if (_handPointCollectionPool.Count != 0) {
          collection = _handPointCollectionPool.Pop();
        } else {
          collection = new HandPointCollection();
        }

        collection.init(warper);
        return collection;
      }

      public static void Return(HandPointCollection handPointCollection) {
        _handPointCollectionPool.Push(handPointCollection);
      }

      private HandPointCollection() {
        _localPositions = new Vector3[NUM_FINGERS * NUM_BONES];
      }

      private void init(RigidbodyWarper warper) {
        Vector3 interactionPosition = warper.RigidbodyPosition;
        Quaternion interactionRotation = warper.RigidbodyRotation;
        _inverseTransformMatrix = Matrix4x4.TRS(interactionPosition, interactionRotation, Vector3.one).inverse;
      }

      public void SetGlobalPosition(Vector3 globalPosition, Finger.FingerType fingerType, Bone.BoneType boneType) {
        _localPositions[getIndex(fingerType, boneType)] = _inverseTransformMatrix.MultiplyPoint3x4(globalPosition);
      }

      public Vector3 GetLocalPosition(Finger.FingerType fingerType, Bone.BoneType boneType) {
        return _localPositions[getIndex(fingerType, boneType)];
      }

      private int getIndex(Finger.FingerType fingerType, Bone.BoneType boneType) {
        return (int)fingerType * 4 + (int)boneType;
      }
    }
  }
}