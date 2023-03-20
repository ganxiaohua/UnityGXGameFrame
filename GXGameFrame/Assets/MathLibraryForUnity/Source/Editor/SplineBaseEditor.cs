using UnityEngine;
using UnityEditor;
using Dest.Math;

public class SplineBaseEditor<T> : Editor
	where T : SplineBase
{
	private T                  _spline;
	private int                _selectedVertex;
	private int                _adding;
	private SerializedProperty _renderColor;
	private SerializedProperty _creationPlane;

	private void OnEnable()
	{
		_spline = target as T;
		_selectedVertex = -1;
		_renderColor = serializedObject.FindProperty("_renderColor");
		_creationPlane = serializedObject.FindProperty("_creationPlane");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(_renderColor);

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Vertex Count", _spline.VertexCount.ToString());
		bool gotSelectedVertex = _selectedVertex != -1 && _selectedVertex < _spline.VertexCount;
		if (gotSelectedVertex)
		{
			EditorGUILayout.LabelField("Selected Vertex", _selectedVertex.ToString());
			Vector3 localPosition = _spline.GetVertex(_selectedVertex);
			Vector3 temp = EditorGUILayout.Vector3Field("Local Position", localPosition);
			if (localPosition != temp)
			{
				MoveVertex(_selectedVertex, temp);
			}

			Vector3 worldPosition = _spline.transform.TransformPoint(localPosition);
			temp = EditorGUILayout.Vector3Field("World Position", worldPosition);
			if (worldPosition != temp)
			{
				temp = _spline.transform.InverseTransformPoint(temp);
				MoveVertex(_selectedVertex, temp);
			}
		}
		else
		{
			EditorGUILayout.LabelField("Vertex is not selected");
			GUI.enabled = false;
			EditorGUILayout.Vector3Field("Local Position", Vector3.zero);
			EditorGUILayout.Vector3Field("World Position", Vector3.zero);
			GUI.enabled = true;
		}

		EditorGUILayout.Space();
		SplineTypes prevType = _spline.SplineType;
		SplineTypes currType = (SplineTypes)EditorGUILayout.EnumPopup("SplineType", prevType);
		if (prevType != currType)
		{
			SetType(currType);
		}
		EditorGUILayout.PropertyField(_creationPlane);
		
		EditorGUILayout.Space();
		bool temp1 = GUILayout.Toggle(_adding == 1, _adding == 1 ?
			new GUIContent("Stop Adding", "Click to stop adding vertices") :
			new GUIContent("Add First", "Click to begin adding vertices to the beginning of the spline"), GUI.skin.button);
		if (temp1) _adding = 1;
		bool temp2 = GUILayout.Toggle(_adding == 2, _adding == 2 ?
			new GUIContent("Stop Adding", "Click to stop adding vertices") :
			new GUIContent("Add Last", "Click to begin adding vertices to the end of the spline"), GUI.skin.button);
		if (temp2) _adding = 2;
		if (!temp1 && !temp2) _adding = 0;

		GUI.enabled = gotSelectedVertex;
		{
			EditorGUILayout.Space();
			if (GUILayout.Button(new GUIContent("Split Before", "Insert new vertex between current and previous vertices")))
			{
				InsertBeforeVertex(_selectedVertex);
			}
			if (GUILayout.Button(new GUIContent("Split After", "Insert new vertex between current and next vertices")))
			{
				InsertAfterVertex(_selectedVertex);
			}

			EditorGUILayout.Space();
			if (GUILayout.Button(new GUIContent("Delete Vertex", "Click to delete currently selected vertex")))
			{
				DeleteVertex(_selectedVertex);
			}
		}
		GUI.enabled = true;
		if (GUILayout.Button(new GUIContent("Clear Spline", "Click to remove all vertices")))
		{
			Clear();
		}

		// This is used for testing. When everything is working right,
		// force updating should not affect the spline in any way,
		// becase all changes are handled inside spline methods, however
		// if pressing force update changes the spline, then there is
		// an error in the code.
		//if (GUILayout.Button("Force Update")) _spline._ForceUpdate();

		if (EditorGUI.EndChangeCheck())
		{
			SetSplineDirty();
		}
		serializedObject.ApplyModifiedProperties();
	}

	private void SetType(SplineTypes type)
	{
		RecordSpline("Set spline type");
		_spline.SplineType = type;
		SetSplineDirty();
	}

	private void MoveVertex(int index, Vector3 position)
	{
		RecordSpline("Move vertex");
		_spline.SetVertex(index, position);
		SetSplineDirty();
	}

	private void AddVertexFirst(Vector2 click)
	{
		RecordSpline("Add vertex first");
		_spline._AddVertexFirst(click, Camera.current);
		SetSplineDirty();
	}

	private void AddVertexLast(Vector2 click)
	{
		RecordSpline("Add vertex last");
		_spline._AddVertexLast(click, Camera.current);
		SetSplineDirty();
	}

	private void DeleteVertex(int index)
	{
		RecordSpline("Delete vertex");
		_spline.RemoveVertex(index);
		SetSplineDirty();
	}

	private void Clear()
	{
		RecordSpline("Clear spline");
		_spline.Clear();
		SetSplineDirty();
	}

	private void InsertBeforeVertex(int index)
	{
		RecordSpline("Split before");
		if (_spline.SplineType == SplineTypes.Open)
		{
			if (index > 0)
			{
				Vector3 point = (_spline.GetVertex(index) + _spline.GetVertex(index - 1)) * .5f;
				_spline.InsertBefore(index, point);
				SetSplineDirty();
			}
		}
		else
		{
			int prevIndex = index - 1;
			if (prevIndex < 0) prevIndex += _spline.VertexCount;
			Vector3 point = (_spline.GetVertex(index) + _spline.GetVertex(prevIndex)) * .5f;
			_spline.InsertBefore(index, point);
			SetSplineDirty();
		}
	}

	private void InsertAfterVertex(int index)
	{
		RecordSpline("Split after");
		if (_spline.SplineType == SplineTypes.Open)
		{
			if (index < _spline.VertexCount - 1)
			{
				Vector3 point = (_spline.GetVertex(index) + _spline.GetVertex(index + 1)) * .5f;
				_spline.InsertAfter(index, point);
				SetSplineDirty();
			}
		}
		else
		{
			int nextIndex = index + 1;
			if (nextIndex >= _spline.VertexCount) nextIndex -= _spline.VertexCount;
			Vector3 point = (_spline.GetVertex(index) + _spline.GetVertex(nextIndex)) * .5f;
			_spline.InsertAfter(index, point);
			SetSplineDirty();
		}
	}

	
	private void SetSplineDirty()
	{
		EditorUtility.SetDirty(_spline);
	}

	private void RecordSpline(string operationName)
	{
		Undo.RecordObject(_spline, operationName);
	}

	
	public void OnSceneGUI()
	{
		Transform splineTransform = _spline.transform;
		if (_selectedVertex != -1 && _selectedVertex < _spline.VertexCount)
		{
			Vector3 pointIn = splineTransform.TransformPoint(_spline.GetVertex(_selectedVertex));
			Vector3 pointOut = splineTransform.InverseTransformPoint(Handles.PositionHandle(pointIn, Quaternion.identity));
			if (pointIn != pointOut)
			{
				MoveVertex(_selectedVertex, pointOut);
			}
			// Handles.DotCap(-1, pointIn, Quaternion.identity, HandleUtility.GetHandleSize(pointIn) * .1f);
		}

		Event current = Event.current;
		if (current.type == EventType.MouseDown)
		{
			if (current.button == 0)
			{
				if (_adding != 0)
				{
					if (_adding == 1)
					{
						AddVertexFirst(current.mousePosition);
					}
					else
					{
						AddVertexLast(current.mousePosition);
					}
					current.Use();
				}
				else
				{
					int selected = _spline._SelectVertex(current.mousePosition, Camera.current);
					if (selected != -1)
					{
						_selectedVertex = selected;
						current.Use();
					}
					else
					{
						_selectedVertex = -1;
					}
					Repaint();
				}
			}
			else if (current.button == 1)
			{
				int index = _spline._SelectVertex(current.mousePosition, Camera.current);
				if (index != -1)
				{
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Delete Vertex"), false, () => DeleteVertex(index));
					menu.AddItem(new GUIContent("Split Before"), false, () => InsertBeforeVertex(index));
					menu.AddItem(new GUIContent("Split After"), false, () => InsertAfterVertex(index));
					menu.ShowAsContext();
					current.Use();
				}
				else
				{
					if (_adding != 0)
					{
						_adding = 0;
						current.Use();
						Repaint();
					}
				}
			}
		}

		if (_adding != 0)
		{
			Selection.activeGameObject = _spline.gameObject;
		}
	}
}
