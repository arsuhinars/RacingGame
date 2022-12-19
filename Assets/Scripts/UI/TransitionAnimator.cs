using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ValueTransitionStorage : SerializableDictionary.Storage<ValueTransition[]> { }

[Serializable]
public class StringValueTransitionDictionary : SerializableDictionary<string, ValueTransition[], ValueTransitionStorage> { }

public class TransitionAnimator : MonoBehaviour
{
    public StringValueTransitionDictionary animations;
    public bool isRealtime = false;
    [Min(0f)] public float timeScale = 1f;

    public bool IsPlaying { get; private set; }
    public string CurrentAnimationName { get; private set; }

    private Action currentTransitionEndHandler;
    private bool isCurrAnimLooped;
    private ValueTransition[] currAnimTransitions;
    private int currTransIndex;
    private readonly LinkedList<ValueTransition> activeTransitions = new();

    public void Play(string name) => Play(name, null);

    public void Play(string name, bool isLooped = false) => Play(name, null, isLooped);

    public void Play(string name, Action transitionEndHandler, bool isLooped=false)
    {
        ValueTransition[] transitions;
        try
        {
            transitions = animations[name];
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError($"Can't find animation with name {name}", this);
            return;
        }

        if (IsPlaying)
        {
            currTransIndex = 0;
            activeTransitions.Clear();
            CurrentAnimationName = "";
            currentTransitionEndHandler?.Invoke();
        }

        IsPlaying = true;
        currentTransitionEndHandler = transitionEndHandler;
        isCurrAnimLooped = isLooped;
        currAnimTransitions = transitions;
        CurrentAnimationName = name;
    }

    public void Stop()
    {
        if (IsPlaying)
        {
            IsPlaying = false;
            currTransIndex = 0;
            activeTransitions.Clear();
            CurrentAnimationName = "";
            currentTransitionEndHandler?.Invoke();
        }
    }

    public void EndAnimationImmediately()
    {
        if (IsPlaying)
        {
            foreach (var trans in currAnimTransitions)
            {
                trans.HandleTransition(1f);
            }
            IsPlaying = false;
            currTransIndex = 0;
            activeTransitions.Clear();
            CurrentAnimationName = "";
            currentTransitionEndHandler?.Invoke();
        }
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        bool didAllEnd = true;
        foreach (var trans in activeTransitions)
        {
            trans.Update();
            if (!trans.DidEnd)
                didAllEnd = false;
        }

        if (didAllEnd)
            activeTransitions.Clear();

        // Если анимация закончилась
        if (currTransIndex >= currAnimTransitions.Length && didAllEnd)
        {
            currTransIndex = 0;
            if (!isCurrAnimLooped)
            {
                IsPlaying = false;
                CurrentAnimationName = "";
                currentTransitionEndHandler?.Invoke();
            }
            return;
        }

        if (currTransIndex < currAnimTransitions.Length)
        {
            var currTrans = currAnimTransitions[currTransIndex];
            if ((!currTrans.playWithPrevious && didAllEnd) || currTrans.playWithPrevious)
            {
                currTrans.time = 0f;
                currTrans.timeScale = timeScale;
                currTrans.isRealtime = isRealtime;

                activeTransitions.AddLast(currTrans);
                currTransIndex++;
            }
        }
    }
}
