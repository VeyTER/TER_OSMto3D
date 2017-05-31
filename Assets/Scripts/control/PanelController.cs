using UnityEngine;
using System.Collections;
using System;

public abstract class PanelController : MonoBehaviour {
	/// <summary>
	/// 	Les états du panneau latéral. Celui-ci peut-être fermé ou ouvert, mais également en cours d'ouverture ou
	/// 	de fermeture.
	/// </summary>
	public enum PanelStates { CLOSED, CLOSED_TO_OPEN, OPEN, OPEN_TO_CLOSED }

	/// <summary>Etat courant du panneau latéral.</summary>
	protected PanelStates panelState;

	protected float startPosX;
	protected float endPosX;


	/// <summary>
	/// 	Ouvre la panneau latéral.
	/// </summary>
	/// <param name="finalAction">Action finale à effectuer à la fin de l'ouverture.</param>
	public void OpenPanel(Action finalAction) {
		panelState = PanelStates.CLOSED_TO_OPEN;
		this.StartCoroutine( this.SlidePanel(finalAction, -1) );
	}


	/// <summary>
	/// 	Ferme la panneau latéral.
	/// </summary>
	/// <param name="finalAction">Action finale à effectuer à la fin de la fermeture.</param>
	public void ClosePanel(Action finalAction) {
		panelState = PanelStates.OPEN_TO_CLOSED;
		this.StartCoroutine( this.SlidePanel(finalAction, 1) );
	}


	/// <summary>
	/// 	Fait glisser le panneau latéral d'une configuration à une autre.
	/// </summary>
	/// <returns>Temporisateur servant à générer une animation.</returns>
	/// <param name="finalAction">Action finale à effectuer à la fin de la glissade.</param>
	/// <param name="direction">Direction de la glissade.</param>
	private IEnumerator SlidePanel(Action finalAction, int direction) {
		// Configuration courante du panneau
		Vector3 panelPosition = transform.localPosition;
		RectTransform panelRectTransform = (RectTransform) transform;

		// Position en X initiale puis ciblée du panneau
		float panelInitPosX = panelPosition.x;

		float panelTargetPosX = panelInitPosX;
		if (direction > 0)
			panelTargetPosX = endPosX;
		else if(direction < 0)
			panelTargetPosX = startPosX;

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			// Calcul et affectation de la situation courante
			float panelCurrentPosX = panelInitPosX - (panelInitPosX - panelTargetPosX) * cursor;
			transform.localPosition = new Vector3(panelCurrentPosX, panelPosition.y, panelPosition.z);

			yield return new WaitForSeconds(0.01F);
		}

		transform.localPosition = new Vector3(panelTargetPosX, panelPosition.y, panelPosition.z);

		if (panelState == PanelStates.CLOSED_TO_OPEN)
			panelState = PanelStates.OPEN;
		else if (panelState == PanelStates.OPEN_TO_CLOSED)
			panelState = PanelStates.CLOSED;

		// Appel de la tâche finale s'il y en a une
		if (finalAction != null)
			finalAction();
	}

	public bool IsPanelOpen() {
		return panelState == PanelStates.OPEN;
	}

	public bool IsPanelClosed() {
		return panelState == PanelStates.CLOSED;
	}

	public float StartPosX {
		get { return startPosX; }
		set { startPosX = value; }
	}

	public float EndPosX {
		get { return endPosX; }
		set { endPosX = value; }
	}
}
