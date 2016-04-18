﻿using UnityEngine;
using System.Collections.Generic;
using Leap.Unity.Interaction.CApi;

namespace Leap.Unity.Interaction {

  public abstract class IInteractionBehaviour : MonoBehaviour {
    /// <summary>
    /// Gets or sets the manager this object belongs to.
    /// </summary>
    public abstract InteractionManager Manager { get; set; }

    /// <summary>
    /// Gets whether or not the behaviour is currently registered with the manager.
    /// </summary>
    public abstract bool IsRegisteredWithManager { get; }

    /// <summary>
    /// Gets the handle to the internal shape description.
    /// </summary>
    public abstract INTERACTION_SHAPE_DESCRIPTION_HANDLE ShapeDescriptionHandle { get; }

    /// <summary>
    /// Gets the handle to the internal shape instance.
    /// </summary>
    public abstract INTERACTION_SHAPE_INSTANCE_HANDLE ShapeInstanceHandle { get; }

    /// <summary>
    /// Returns true if there is at least one hand grasping this object.
    /// </summary>
    public abstract bool IsBeingGrasped { get; }

    /// <summary>
    /// Returns the number of hands that are currently grasping this object.
    /// </summary>
    public abstract int GraspingHandCount { get; }

    /// <summary>
    /// Returns the number of hands that are currently grasping this object but
    /// are not currently being tracked.
    /// </summary>
    public abstract int UntrackedHandCount { get; }

    /// <summary>
    /// Returns the ids of the hands currently grasping this object.
    /// </summary>
    public abstract IEnumerable<int> GraspingHands { get; }

    /// <summary>
    /// Returns the ids of the hands currently grasping this object, but only
    /// returns ids of hands that are also currently being tracked.
    /// </summary>
    public abstract IEnumerable<int> TrackedGraspingHands { get; }

    /// <summary>
    /// Returns the ids of the hands that are considered grasping but are untracked.
    /// </summary>
    public abstract IEnumerable<int> UntrackedGraspingHands { get; }

    /// <summary>
    /// Returns whether or not a hand with the given ID is currently grasping this object.
    /// </summary>
    public abstract bool IsBeingGraspedByHand(int handId);



    /// <summary>
    /// Called by InteractionManager when the behaviour is successfully registered with the manager.
    /// </summary>
    public abstract void OnRegister();

    /// <summary>
    /// Called by InteractionManager when the behaviour is unregistered with the manager.
    /// </summary>
    public abstract void OnUnregister();

    /// <summary>
    /// Called by InteractionManager before any solving is performed.
    /// </summary>
    public abstract void OnPreSolve();

    /// <summary>
    /// Called by InteractionManager after all solving is performed.
    /// </summary>
    public abstract void OnPostSolve();

    /// <summary>
    /// Called by InteractionManager to get the creation info used to create the shape associated with this InteractionBehaviour.
    /// </summary>
    /// <param name="createInfo"></param>
    /// <param name="createTransform"></param>
    public abstract void OnInteractionShapeCreationInfo(out INTERACTION_CREATE_SHAPE_INFO createInfo, out INTERACTION_TRANSFORM createTransform);

    /// <summary>
    /// Called by InteractionManager when the interaction shape associated with this InteractionBehaviour
    /// is created and added to the interaction scene.
    /// </summary>
    public abstract void OnInteractionShapeCreated(INTERACTION_SHAPE_INSTANCE_HANDLE instanceHandle);

    /// <summary>
    /// Called by InteractionManager when the interaction shape associated with this InteractionBehaviour
    /// is about to be updated.
    /// </summary>
    public abstract void OnInteractionShapeUpdate(out INTERACTION_UPDATE_SHAPE_INFO updateInfo, out INTERACTION_TRANSFORM interactionTransform);

    /// <summary>
    /// Called by InteractionManager when the interaction shape associated with this InteractionBehaviour
    /// is destroyed and removed from the interaction scene.
    /// </summary>
    public abstract void OnInteractionShapeDestroyed();

    /// <summary>
    /// Called by InteractionManager when the velocity of an object is changed.
    /// </summary>
    public abstract void OnRecieveSimulationResults(INTERACTION_SHAPE_INSTANCE_RESULTS results);

    /// <summary>
    /// Called by InteractionManager when a Hand begins grasping this object.
    /// </summary>
    public abstract void OnHandGrasp(Hand hand);

    /// <summary>
    /// Called by InteractionManager every frame that a Hand continues to grasp this object.  This callback
    /// is invoked both in FixedUpdate, and also in LateUpdate.  This gives all objects a chance to both update
    /// their physical representation as well as their graphical, for decreased latency and increase fidelity.
    /// </summary>
    public abstract void OnHandsHold(List<Hand> hands);

    /// <summary>
    /// Called by InteractionManager when a Hand stops grasping this object.
    /// </summary>
    public abstract void OnHandRelease(Hand hand);

    /// <summary>
    /// Called by InteractionManager when a Hand that was grasping becomes untracked.  The Hand
    /// is not yet considered ungrasped, and OnHandRegainedTracking might be called in the future
    /// if the Hand becomes tracked again.
    /// </summary>
    public abstract void OnHandLostTracking(Hand oldHand);

    /// <summary>
    /// Called by InteractionManager when a grasping Hand that had previously been untracked has
    /// regained tracking.  The new hand is provided, as well as the id of the previously tracked
    /// hand.
    /// </summary>
    public abstract void OnHandRegainedTracking(Hand newHand, int oldId);

    /// <summary>
    /// Called by InteractionManager when an untracked grasping Hand has remained ungrasped for
    /// too long.  The hand is no longer considered to be grasping the object.
    /// </summary>
    public abstract void OnHandTimeout(Hand oldHand);

  }
}