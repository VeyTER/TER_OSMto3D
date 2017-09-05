using UnityEngine;
using System.Collections.Generic;

public class DevicesAi {
	private List<CommandModule> commandModules;

	public DevicesAi() {
		this.commandModules = new List<CommandModule>();
	}

	public void AddUserInteraction(string actuatorPath, Object data) {

	}

	public void UpdateActuators(string actuatorPath, Object data) {

	}

	public CommandModule AddCommandModule(string sensorPath, string actuatorPath, Gradient.GradientsTypes gradientTypefloat, int gradientSize) {
		CommandModule newModule = new CommandModule(sensorPath, actuatorPath, gradientTypefloat, gradientSize);
		commandModules.Add(newModule);
		return newModule;
	}

	public CommandModule AddCommandModule(string sensorPath, string actuatorPath, Gradient.GradientsTypes gradientTypefloat, Vector2 gradientSize) {
		CommandModule newModule = new CommandModule(sensorPath, actuatorPath, gradientTypefloat, gradientSize);
		commandModules.Add(newModule);
		return newModule;
	}

	public void AddCommandModule(CommandModule newModule) {
		commandModules.Add(newModule);
	}

	public CommandModule GetCommandModule(string sensorPath, string actuatorPath) {
		int i = 0;
		for (; i < commandModules.Count && !(commandModules[i].SensorPath.Equals(sensorPath) && commandModules[i].ActuatorPath.Equals(actuatorPath)); i++) ;

		if (commandModules[i] != null)
			return commandModules[i];
		else
			return null;
	}

	public CommandModule RemoveCommandModule(string sensorPath, string actuatorPath) {
		CommandModule oldModule = this.GetCommandModule(sensorPath, actuatorPath);
		if(oldModule != null)
			commandModules.Remove(oldModule);
		return oldModule;
	}

	public void RemoveCommandModule(CommandModule oldModule) {
		if (oldModule != null)
			commandModules.Remove(oldModule);
	}
}