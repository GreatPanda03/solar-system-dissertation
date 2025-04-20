## Changelog

This changelog covers a list of features and fixes which have been made for the application.

Versions are in the form **X.Y.Z**: *X* represents a significant feature which covers a range of functionality; *Y* represents single / small feature changes; and *Z* represents fixes or build-ups to the same feature as a gradual release.

All dates are in the form **YYYY/MM/DD**.

---

### Version: 1.5.1
**Date:** 2025/04/20
<br/>**Bug Fixes:**
* **1)** Trajectories for launching and stable orbits now scale correctly
* **2)** Deselecting a body now always hides the properties window *(as intended)*
* **3)** Stable orbits around moving bodies are now properly calculated *(initial velocity of "parent" considered)*
* **Note:** the camera still zooms relative to the origin

**Additions / Changes:**
* **1)** Property window values update per frame rather than on body selection
* **2)** Stable orbit paths are now fixed circles around the "parent" body *(for "limitless" size)*

---

### Version: 1.5.0
**Date:** 2025/04/20
<br/>**Additions / Changes:**
* **1)** Added a basic ability to zoom with the scrollwheel to scale the system
* **Note 1:** trajectories at a scale other than x1 don't scale as they should yet *(mis-sized trajectory velocities and positions)*
* **Note 2:** the camera allow zooms relative to the origin, not the current camera position

---

### Version: 1.4.3
**Date:** 2025/04/19
<br/>**Additions / Changes:**
* **1)** Placing a body in Orbit mode while following another body makes the orbit relative to the followed body
* **2)** Temporarily removed checks for mouse over UI *(was inconsistent)*

---

### Version: 1.4.2
**Date:** 2025/04/19
<br/>**Additions / Changes:**
* **1)** "Orbit" mode allows bodies to be placed and fired in an orbit around the closest body
* **2)** New visual orbit around attractor when preparing to place a body in orbit
* **3)** New visual direction indicator when preparing to fire a body at a trajectory

---

### Version: 1.4.1
**Date:** 2025/04/19
<br/>**Bug Fixes:**
* **1)** Fixed the properties & explorer windows appearing at the start of the simulation
* **2)** Fixed not being able to close the properties & explorer windows

**Additions / Changes:**
* **1)** "Plane" mode allows bodies to be placed and fired on a plane *(Y = 0)*
* **2)** New plane visual when selecting "Plane" mode *(only for "Plane" mode)*

---

### Version: 1.4.0
**Date:** 2025/04/19
<br/>**Additions / Changes:**
* **1)** New mode panel to select a placement mode *(free, plane, or orbit placement)*
* **2)** New mode button to toggle the mode panel
* **3)** Code reorganisation to accommodate placement types
* **4)** Can now use the mouse scroll wheel to zoom in / out
* **5)** Zooming with a body preview moves the entire camera rather than the preview
* **Note:** only "Free" mode works - "Plane" and "Orbit" mode are a WIP

---

### Version: 1.3.1
**Date:** 2025/04/18
<br/>**Bug Fixes:**
* **1)** Fixed body not being followed aftering selecting it in the explorer

**Additions / Changes:**
* **1)** Editing a body's details now updates the corresponding entry in the explorer

---

### Version: 1.3.0
**Date:** 2025/04/18
<br/>**Bug Fixes:**
* **1)** Closing the properties window while following a body stops following the body
* **2)** The camera no longer moves when pressing movement keys while a textbox is selected

**Additions / Changes:**
* **1)** Creating a body created a new entry in the explorer when adding a body
* **2)** Selecting an entry in the explorer follows the corresponding body
* **Note:** editing a body's properties doesn't update its explorer entry yet

---

### Version: 1.2.0
**Date:** 2025/04/18
<br/>**Bug Fixes:**
* **1)** Selecting a text box with a preview body selected no longer places a body behind the UI

**Additions / Changes:**
* **1)** New Explorer panel (not yet usable)
* **2)** New panel to distinguish between Body List and Explorer
* **3)** Can follow a planet when selecting a planet *(follows the planet until deselected)*
* **4)** Added a new background plane texture (currently invisible)

---

### Version: 1.1.0
**Date:** 2025/02/12
<br/>**Additions / Changes:**
* **1)** Simulations can be saved
* **Note:** although saving works, loading doesn't work properly yet

---

### Version: 1.0.0
**Date:** 2025/02/09
<br/>**Additions / Changes:**
* **1)** **Main menu** - primary area to access different parts of the application
    * Currently, only "New Simulation" works
* **2)** New blank simulations can be created with a small set of settings
    * Simulation Name - the name of the simulation 
    * Gravity Constant - gravity strength
    * Gravity Algorithm - how gravity is simulated
* **3)** Stars and Planets can be created in simulations
    * Currently, Stars and Planets have a fixed mass
* **4)** Gravity is simulated with Newtonian Physics (this is the only algorithm for now)
* **5)** Movement controls - a camera which can be moved and turned to explore the system

---