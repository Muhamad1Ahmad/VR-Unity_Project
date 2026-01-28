# VR Fire Safety Training – Lithium-Ion Battery Fire

## Project Overview
This project presents an immersive Virtual Reality (VR) training simulation focused on handling lithium-ion battery fires in a server room environment. Lithium-ion fires pose unique hazards due to thermal runaway, toxic fumes, and the ineffectiveness of conventional water-based fire suppression methods.  

The goal of this project is to bridge the gap between theoretical fire safety instruction and real-world procedural training by providing a **safe-to-fail**, interactive VR experience. Users are required to correctly identify the fire source, trigger safety mechanisms, select the appropriate extinguisher, and perform the correct extinguishing procedure under time pressure.

The project was developed as part of **Project CSI (Visual Computing), Winter Semester 2025/26**.

---

## Key Features
- Immersive VR-based fire safety training scenario
- Safe-to-fail design allowing users to learn from mistakes
- Gesture-based interaction using hand tracking
- Context-aware guidance without breaking immersion
- Physically realistic yet semantically controlled fire extinguishing logic
- Robust state management for success, failure, and replayability

---

## Scenario Description
The training scenario follows a structured sequence:

1. **Identify** the lithium-ion battery fire source
2. **Trigger the alarm** using a physical interaction
3. **Select the correct extinguisher** (Class D) while ignoring incorrect options
4. **Execute the extinguishing procedure** using correct gestures (pin pull, aim, spray)
5. **Receive feedback** based on user performance (success or failure)

Incorrect actions (e.g., using the wrong extinguisher or running out of time) result in scenario failure, allowing users to safely retry and learn.

---

## Technical Architecture
The system is organized into four conceptual layers:

### 1. Scenario State & Flow Control
Responsible for managing timers, win/fail conditions, and scene resets.
- `AlarmCountdownGameManager.cs`
- `SceneRestarter.cs`
- `XROriginRespawnOnLoad.cs`

### 2. Physical Interaction & Semantic Logic
Separates physical object interaction from logical training semantics.
- `FireExtinguishZone.cs`
- `FireStartPrompt.cs`
- `ExtinguisherTutorial.cs`
- `KeepUprightWhileGrabbed.cs`

### 3. Guidance & Training UX
Provides contextual user guidance and environmental feedback.
- `StepDialogueUI.cs`
- `ProximityGuidancePrompt.cs`
- `AlarmLightPulser.cs`
- `CCTVConnector.cs`

### 4. XR Interaction Robustness
Ensures stable and reliable gesture recognition in VR.
- `GestureStabilizer.cs`

---

## Task Breakdown & Responsibilities
The project was developed collaboratively with clearly defined focus areas:

| Task Area | Responsible |
|---------|------------|
| Interaction logic & state management | Saad Sameer Khan |
| Training guidance & dialogue systems | Saad Sameer Khan |
| XR gesture stability & interaction robustness | Muhammad Ahmad |
| Environment setup & visual feedback | Muhammad Ahmad |
| Interaction ergonomics & realism | Muhammad Ahmad |
| Integration & testing | Both |

---

## Installation & Build Instructions

### Requirements
- **Unity 2022 LTS**
- **Meta Quest 2 / Quest 3**
- **Meta XR SDK**
- **OpenXR Plugin**
- **XR Interaction Toolkit**

### Setup Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/Muhamad1Ahmad/VR-Unity_Project.git
2. Ensure the following packages are enabled:
   - OpenXR
   - Meta XR Core SDK
   - Meta XR Interaction SDK
   - XR Interaction Toolkit
3. Enable **Hand Tracking** in:
   - OpenXR settings
   - Meta XR project settings
4. Switch the build platform to **Android**
5. Build and deploy the project to the Meta Quest device

---

## External Libraries & Frameworks
- Unity Engine 2022 LTS
- Meta XR SDK
- OpenXR
- XR Interaction Toolkit
- TextMeshPro

---

## AI Usage Disclaimer
AI-based tools (including large language models) were used solely as **development support tools** for:
- Code structuring suggestions
- Debugging assistance
- Documentation drafting support

All architectural decisions, implementation logic, debugging, and final integration were performed and validated by the project authors. No AI-generated code was used without review, modification, and understanding by the developers.

