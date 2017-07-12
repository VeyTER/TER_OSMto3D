using UnityEngine;
using System;

/// <summary>
///		Classe-mère contrôlant un panneau d'éléments graphiques et permettant de l'animer.
/// </summary>
public abstract class PanelController : MonoBehaviour {
	/// <summary>
	/// 	Les états du panneau latéral. Celui-ci peut-être fermé ou ouvert, mais également en cours d'ouverture ou
	/// 	de fermeture.
	/// </summary>
	public enum PanelStates { CLOSED, CLOSED_TO_OPEN, OPEN, OPEN_TO_CLOSED }

	/// <summary>Etat courant du panneau latéral.</summary>
	protected PanelStates panelState;

	protected Vector3 startPosition;
	protected Vector3 endPosition;

	/// <summary>
	/// 	Ouvre la panneau latéral.
	/// </summary>
	/// <param name="finalAction">Action finale à effectuer à la fin de l'ouverture.</param>
	public abstract void OpenPanel(Action finalAction);


	/// <summary>
	/// 	Ferme la panneau latéral.
	/// </summary>
	/// <param name="finalAction">Action finale à effectuer à la fin de la fermeture.</param>
	public abstract void ClosePanel(Action finalAction);

	public bool IsPanelOpen() {
		return panelState == PanelStates.OPEN;
	}

	public bool IsPanelClosed() {
		return panelState == PanelStates.CLOSED;
	}

	public Vector3 StartPosition {
		get { return startPosition; }
		set { startPosition = value; }
	}

	public Vector3 EndPosition {
		get { return endPosition; }
		set { endPosition = value; }
	}
}
