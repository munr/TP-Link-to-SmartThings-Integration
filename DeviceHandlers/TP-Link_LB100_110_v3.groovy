/*
TP-Link LB100 and LB110 Device Handler
FOR USE ONLY WITH 'TP-LinkServer_v3.js'

Copyright 2017 Dave Gutheinz

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this 
file except in compliance with the License. You may obtain a copy of the License at:

		http://www.apache.org/licenses/LICENSE-2.0
        
Unless required by applicable law or agreed to in writing, software distributed under 
the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF 
ANY KIND, either express or implied. See the License for the specific language governing 
permissions and limitations under the License.

Supported models and functions:  This device supports the TP-Link LB100 and LB120.

Notes: 
1.	This Device Handler requires an operating Windows 10 PC Bridge running version 3.0 of
	'TP-LinkServer.js'.  It is NOT fully compatible with earlier versions.
2.	This device supports the TP-Link LB100 and LB110 devices.  It supports the on/off and
	bightness functions.

Update History
	06/01/2017	- Initial release of Version 3.0.  Following add-ons from version 2.3
				  1.  Compatible with SmartTiles / ActionTiles
                  2.  Added error messaging to final version.
                  3.  Added error messages to appear in ST phone app, Recently tab.
                  4.  Modified coloring and added a Waiting state that displays whenever
                  	  a function is actuated to indicate waiting for response from the
                      Bridge.
*/

metadata {
	definition (
    	name: "TP-Link LB100 and 110", namespace: "djg", author: "Dave Gutheinz") {
		capability "Switch"
		capability "Switch Level"
		capability "refresh"
        capability "Sensor"
		capability "Actuator"
	}
	tiles(scale: 2) {
		multiAttributeTile(name:"switch", type: "lighting", width: 6, height: 4, canChangeIcon: true){
			tileAttribute ("device.switch", key: "PRIMARY_CONTROL") {
				attributeState "on", label:'${name}', action:"switch.off", icon:"st.switches.light.on", backgroundColor:"#00a0dc",
				nextState:"waiting"
				attributeState "off", label:'${name}', action:"switch.on", icon:"st.switches.light.off", backgroundColor:"#ffffff",
				nextState:"waiting"
				attributeState "waiting", label:'${name}', action:"switch.on", icon:"st.switches.light.on", backgroundColor:"#15EE10",
				nextState:"on"
                attributeState "offline", label:'Comms Error', action:"switch.on", icon:"st.switches.switch.off", backgroundColor:"#e86d13",
                nextState:"waiting"
			}
			tileAttribute ("device.level", key: "SLIDER_CONTROL") {
				attributeState "level", label: "Brightness: ${currentValue}", action:"switch level.setLevel"
			}
		}
		standardTile("refresh", "capability.refresh", width: 2, height: 2,  decoration: "flat") {
			state ("default", label:"Refresh", action:"refresh.refresh", icon:"st.secondary.refresh", backgroundColor:"#ffffff")
		}         
		main("switch")
		details(["switch", "refresh"])
    }
}
preferences {
	input("deviceIP", "text", title: "Device IP", required: true, displayDuringSetup: true)
	input("gatewayIP", "text", title: "Gateway IP", required: true, displayDuringSetup: true)
}
def on() {
	sendCmdtoServer('{"smartlife.iot.smartbulb.lightingservice":{"transition_light_state":{"on_off":1}}}', "commandResponse")
}
def off() {
	sendCmdtoServer('{"smartlife.iot.smartbulb.lightingservice":{"transition_light_state":{"on_off":0}}}', "commandResponse")
}
def setLevel(percentage) {
	percentage = percentage as int
    sendCmdtoServer("""{"smartlife.iot.smartbulb.lightingservice":{"transition_light_state":{"ignore_default":1,"on_off":1,"brightness":${percentage}}}}""", "commandResponse")
}
def refresh(){
	sendCmdtoServer('{"system":{"get_sysinfo":{}}}', "refreshResponse")
}
private sendCmdtoServer(command, action){
	sendEvent(name: "switch", value: "waiting", isStateChange: true)
	def headers = [:] 
	headers.put("HOST", "$gatewayIP:8082")   // port 8082 must be same as value in TP-LInkServerLite.js
	headers.put("tplink-iot-ip", deviceIP)
    headers.put("tplink-command", command)
	headers.put("command", "deviceCommand")
	sendHubCommand(new physicalgraph.device.HubAction([
		headers: headers],
		device.deviceNetworkId,
		[callback: action]
	))
}
def commandResponse(response){
	if (response.headers["cmd-response"] == "TcpTimeout") {
		log.error "$device.name $device.label: Communications Error"
		sendEvent(name: "switch", value: "offline", descriptionText: "ERROR - OffLine - mod commandResponse", isStateChange: true)
     } else {
		def cmdResponse = parseJson(response.headers["cmd-response"])
		state =  cmdResponse["smartlife.iot.smartbulb.lightingservice"]["transition_light_state"]
		parseStatus(state)
	}
}
def refreshResponse(response){
	if (response.headers["cmd-response"] == "TcpTimeout") {
		log.error "$device.name $device.label: Communications Error"
		sendEvent(name: "switch", value: "offline", descriptionText: "ERROR - OffLine - mod refreshResponse", isStateChange: true)
     } else {
		def cmdResponse = parseJson(response.headers["cmd-response"])
		state = cmdResponse.system.get_sysinfo.light_state
		parseStatus(state)
	}
}
def parseStatus(state){
	def status = state.on_off
	if (status == 1) {
		status = "on"
	} else {
		status = "off"
		state = state.dft_on_state
	}
	def level = state.brightness
	log.info "$device.name $device.label: Power: ${status} / Brightness: ${level}%"
	sendEvent(name: "switch", value: status, isStateChange: true)
	sendEvent(name: "level", value: level, isStateChange: true)
}