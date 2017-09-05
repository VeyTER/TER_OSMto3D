using System.Collections.Generic;

public class DevicesSimulationBuilder {
	private static DevicesSimulationBuilder instance;

	public static DevicesSimulationBuilder GetInstance() {
		if (instance == null)
			instance = new DevicesSimulationBuilder();
		return instance;
	}

	public Dictionary<string, BuildingRoom> BuildSimulation(bool tryGenerateAlert) {
		SensorThreshold temperatureThresholdCf = new SensorThreshold(ThresholdConditions.IN, 0, 25);
		SensorData temperatureSensorCf = new SensorData(3, "U4", "campusfab", "temperature", "Temperature", tryGenerateAlert ? "26.6" : "24.8", "°", IconsTexturesSprites.TEMPERATURE_ICON, temperatureThresholdCf);

		SensorThreshold humidityThresholdCf = new SensorThreshold(ThresholdConditions.IN, 0, 100);
		SensorData humiditySensorCf = new SensorData(4, "U4", "campusfab", "humidity", "Humidity", "27", "%", IconsTexturesSprites.HUMIDITY_ICON, humidityThresholdCf);

		SensorThreshold luminosityThresholdCf = new SensorThreshold(ThresholdConditions.IN, 0, 150);
		SensorData luminositySensorCf = new SensorData(5, "U4", "campusfab", "luminosity", "Luminosity", "125", "lux", IconsTexturesSprites.LUMINOSITY_ICON, luminosityThresholdCf);

		SensorThreshold co2ThresholdCf = new SensorThreshold(ThresholdConditions.IN, 0, 300);
		SensorData co2SensorCf = new SensorData(6, "U4", "campusfab", "co2", "CO2", "253", "ppm", IconsTexturesSprites.CO2_ICON, co2ThresholdCf);

		SensorThreshold presenceThresholdCf = new SensorThreshold(ThresholdConditions.EQUALS, "oui");
		SensorData presenceSensorCf = new SensorData(7, "U4", "campusfab", "presence", "Presence", "oui", "", IconsTexturesSprites.PRESENCE_ICON, presenceThresholdCf);


		SensorThreshold temperatureThreshold302 = new SensorThreshold(ThresholdConditions.IN, 0, 25);
		SensorData temperatureSensor302 = new SensorData(0, "U4", "302", "temperature", "Temperature", "24.2", "°", IconsTexturesSprites.TEMPERATURE_ICON, temperatureThreshold302);

		SensorThreshold humidityThreshold302 = new SensorThreshold(ThresholdConditions.IN, 0, 100);
		SensorData humiditySensor302 = new SensorData(1, "U4", "302", "humidity", "Humidity", "35", "%", IconsTexturesSprites.HUMIDITY_ICON, humidityThreshold302);

		SensorThreshold co2Threshold302 = new SensorThreshold(ThresholdConditions.IN, 0, 300);
		SensorData co2Sensor302 = new SensorData(2, "U4", "302", "co2", "CO2", "279", "ppm", IconsTexturesSprites.CO2_ICON, co2Threshold302);


		ActuatorController temperatureControllerCf = new ActuatorController(9, "U4", "302", "heaters", "24", "°", IconsTexturesSprites.HEATERS_CONTROL_ICON) {
			MinValue = 17,
			MaxValue = 32,
			DefaultStep = 0.5F,
		};

		ActuatorController luminosityControllerCf = new ActuatorController(10, "U4", "302", "shutters", "45", "%", IconsTexturesSprites.SHUTTERS_CONTROL_ICON) {
			MinValue = 0,
			MaxValue = 100,
			DefaultStep = 1
		};

		ActuatorController temperatureController302 = new ActuatorController(8, "U4", "302", "heaters", "25", "°", IconsTexturesSprites.HEATERS_CONTROL_ICON) {
			MinValue = 15,
			MaxValue = 35,
			DefaultStep = 0.5F,
		};

		BuildingRoom roomFabcampus = new BuildingRoom("campusfab");
		roomFabcampus.AddActuatorController(temperatureSensorCf, temperatureControllerCf);
		roomFabcampus.AddSensorData(humiditySensorCf);
		roomFabcampus.AddActuatorController(luminositySensorCf, luminosityControllerCf);
		roomFabcampus.AddSensorData(co2SensorCf);
		roomFabcampus.AddSensorData(presenceSensorCf);

		BuildingRoom room302 = new BuildingRoom("302");
		room302.AddActuatorController(temperatureSensor302, temperatureController302);
		room302.AddSensorData(humiditySensor302);
		room302.AddSensorData(co2Sensor302);

		Dictionary<string, BuildingRoom> buildingRooms = new Dictionary<string, BuildingRoom> {
			{ "campusfab", roomFabcampus },
			{ "302", room302 }
		};

		return buildingRooms;
	}
}