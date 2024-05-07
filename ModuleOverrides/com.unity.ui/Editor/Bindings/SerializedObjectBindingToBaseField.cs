// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

﻿using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UnityEditor.UIElements.Bindings;

abstract class SerializedObjectBindingToBaseField<TValue, TField> : SerializedObjectBindingBase where TField : class, INotifyValueChanged<TValue>
{
    private bool isUpdating;

    EventCallback<ChangeEvent<TValue>> m_FieldValueChanged;

    private static EqualityComparer<TValue> s_EqualityComparer = EqualityComparer<TValue>.Default;

    protected TField field
    {
        get { return m_Field as TField; }
        set
        {
            var ve = field as VisualElement;
            ve?.UnregisterCallback(m_FieldValueChanged, TrickleDown.TrickleDown);
            boundElement = value as IBindable;
            ve = field as VisualElement;
            ve?.RegisterCallback(m_FieldValueChanged, TrickleDown.TrickleDown);
        }
    }

    protected SerializedObjectBindingToBaseField()
    {
        m_FieldValueChanged = FieldValueChanged;
    }

    private void FieldValueChanged(ChangeEvent<TValue> evt)
    {
        if (isReleased || isUpdating)
            return;

        if (evt.target != m_Field)
            return;

        try
        {
            var undoGroup = Undo.GetCurrentGroup();

            var bindable = evt.target as IBindable;
            var binding = bindable?.binding;

            if (binding == this && ResolveProperty())
            {
                if (!isFieldAttached)
                {
                    //we don't update when field is not attached to a panel
                    //but we don't kill binding either
                    return;
                }

                UpdateLastFieldValue();

                if (SyncFieldValueToProperty())
                {
                    bindingContext.UpdateRevision(); //we make sure to Poll the ChangeTracker here
                    bindingContext?.ResetUpdate();
                }

                var fieldUndoGroup = (int?)(field as VisualElement)?.GetProperty(UndoGroupPropertyKey);
                Undo.CollapseUndoOperations(fieldUndoGroup ?? undoGroup);

                BindingsStyleHelpers.UpdateElementStyle(field as VisualElement, boundProperty);

                return;
            }
        }
        catch (NullReferenceException e) when (e.Message.Contains("SerializedObject of SerializedProperty has been Disposed."))
        {
            //this can happen when serializedObject has been disposed of
        }

        // Something was wrong
        Release();
    }

    protected override void ResetCachedValues()
    {
        UpdateLastFieldValue();
        UpdateFieldIsAttached();

        if (field is ObjectField objectField)
        {
            objectField.SetProperty(ObjectField.serializedPropertyKey, boundProperty);
            objectField.UpdateDisplay();
        }
    }

    public override void OnPropertyValueChanged(SerializedProperty currentPropertyIterator)
    {
        if (isReleased)
            return;
        try
        {
            isUpdating = true;
            var veField = field as VisualElement;
            var bindable = field as IBindable;

            if (bindable.binding == this)
            {
                SyncPropertyToField(field, currentPropertyIterator);
                BindingsStyleHelpers.UpdateElementStyle(veField, currentPropertyIterator);
                return;
            }
        }
        catch (NullReferenceException e) when (e.Message.Contains("SerializedObject of SerializedProperty has been Disposed."))
        {
            //this can happen when serializedObject has been disposed of
        }
        finally
        {
            isUpdating = false;
        }
        // We unbind here
        Release();
    }

    protected override void OnFieldAttached()
    {
        var previousValue = field.value;

        base.OnFieldAttached();

        if (!boundProperty.hasMultipleDifferentValues && EqualityComparer<TValue>.Default.Equals(previousValue, field.value)
            && field is VisualElement handler)
        {
            using var evt = ChangeEvent<TValue>.GetPooled(field.value, field.value);
            evt.target = handler;
            handler.SendEvent(evt);
        }
    }

    public override void Update()
    {
        if (isReleased)
            return;

        if (!bindingContext.IsValid())
        {
            Release();
            return;
        }

        try
        {
            ResetUpdate();

            if (!IsSynced())
                return;

            isUpdating = true;

            if (FieldBinding == this)
            {
                var veField = field as VisualElement;

                // Value might not have changed but prefab state could have been reverted, so we need to
                // at least update the prefab override visual if necessary. Happens when user reverts a
                // field where the value is the same as the prefab registered value. Case 1276154.
                BindingsStyleHelpers.UpdatePrefabStateStyle(veField, boundProperty);

                if (EditorApplication.isPlaying && SerializedObject.GetLivePropertyFeatureGlobalState() && boundProperty.isLiveModified)
                    BindingsStyleHelpers.UpdateLivePropertyStateStyle(veField, boundProperty);

                return;
            }
        }
        catch (NullReferenceException e) when (e.Message.Contains("SerializedObject of SerializedProperty has been Disposed."))
        {
            //this can happen when serializedObject has been disposed of
        }
        finally
        {
            isUpdating = false;
        }

        // Something failed, we unbind here
        Release();
    }

    // Read the value from the ui field and save it.
    protected abstract void UpdateLastFieldValue();

    protected abstract bool SyncFieldValueToProperty();
    protected abstract void SyncPropertyToField(TField c, SerializedProperty p);
}
