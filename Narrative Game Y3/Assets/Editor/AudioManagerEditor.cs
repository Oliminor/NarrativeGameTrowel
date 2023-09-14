using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    //  This boolean keeps track of whether or not the 'Sound' Object in the list should be Collapsed or Expanded
    private bool[] soundFoldouts;

    public override void OnInspectorGUI()
    {
        //  This shows any other Variables that are not hidden from the Inspector
        base.OnInspectorGUI();

        //  This Updates the Serialized Object
        serializedObject.Update();

        //  Finds the array of the 'Sound' named sounds in the AudioManager
        var soundList = serializedObject.FindProperty("sounds");

        //  Adds a Bold Lable for the list of sounds 
        EditorGUILayout.LabelField("Sounds", EditorStyles.boldLabel);

        //  This increases the indentation level of the GUI objects that come after
        EditorGUI.indentLevel++;

        //  This makes sure that the size of the soundFoldouts array is equal to the size of the sound list and if not it will adjust with the correct length
        if (soundFoldouts == null || soundFoldouts.Length != soundList.arraySize)
        {
            soundFoldouts = new bool[soundList.arraySize];
        }

        //  this loops throuugh all the Sound objects in the soundList
        for (int i = 0; i < soundList.arraySize; i++)
        {
            //  This finds all the serialized properties for each of the variables on the Sound object
            var sound = soundList.GetArrayElementAtIndex(i);
            var name = sound.FindPropertyRelative("name");
            var clip = sound.FindPropertyRelative("clip");
            var outputAudioMixerGroup = sound.FindPropertyRelative("outputAudioMixerGroup");
            var isDefault = sound.FindPropertyRelative("Default");
            var volume = sound.FindPropertyRelative("volume");
            var pitch = sound.FindPropertyRelative("pitch");
            var spacialBlend = sound.FindPropertyRelative("spacialBlend");
            var loop = sound.FindPropertyRelative("loop");

            //  Begin a vertical box for each sound object
            EditorGUILayout.BeginVertical(GUI.skin.box);
            //  Begin a horizontal layout for each sound object
            EditorGUILayout.BeginHorizontal();

            //  Displays the name of the sound object using the name parameter within the sound object
            soundFoldouts[i] = EditorGUILayout.Foldout(soundFoldouts[i], name.stringValue);

            //  creates a button for removing and deleting the sound object from the sound list
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                soundList.DeleteArrayElementAtIndex(i);
                break;
            }

            // Ends the horizontal layout for each sound object
            EditorGUILayout.EndHorizontal();

            // Displays all variables of the Sound Class that are a part of the sounds array as a dropdown 
            if (soundFoldouts[i])
            {
                EditorGUILayout.PropertyField(name, GUIContent.none);
                EditorGUILayout.PropertyField(clip);
                EditorGUILayout.PropertyField(outputAudioMixerGroup);
                EditorGUILayout.PropertyField(isDefault);

                if (!isDefault.boolValue) // Will only display these variables if Default is false
                {
                    EditorGUILayout.PropertyField(volume);
                    EditorGUILayout.PropertyField(pitch);
                    EditorGUILayout.PropertyField(spacialBlend);
                }

                EditorGUILayout.PropertyField(loop);
            }
            //  Ends the Vertical box each sound object
            EditorGUILayout.EndVertical();
        }
        //  Decreases the indentation level of all GUI objects after it
        EditorGUI.indentLevel--;

        //  Creates button used for creating and adding a sound object to the soundList
        if (GUILayout.Button("+"))
        {
            soundList.InsertArrayElementAtIndex(soundList.arraySize);
            soundFoldouts = new bool[soundList.arraySize];
        }

        //  This Applies any changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
