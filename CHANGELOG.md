## Changelog

This changelog covers a list of features and fixes which have been made for the application.

Versions are in the form **X.Y.Z**: *X* represents a significant feature which covers a range of functionality; *Y* represents single / small feature changes; and *Z* represents fixes or build-ups to the same feature as a gradual release.

All dates are in the form **YYYY/MM/DD**.

---

### Version: 1.8.3
**Date:** 2025/04/25
<br/>**Bug Fixes:**
* **1)** Fixed camera jitter when rotating around a followed body *(smoother transition)*
* **2)** Limited camera pitch range *(to avoid "spasms" when looking directly upwards)*

---

### Version: 1.8.2
**Date:** 2025/04/25
<br>**Bug Fixes:**
* **1)** Added appropriate handling when deleting a followed body

**Additions / Changes:**
* **1)** New "P" keybind to toggle the properties window
* **2)** New "F" keybind to toggle following for the selected body
* **3)** New "V" keybind to set to view a followed body at an appropriate scale

### Version: 1.8.1
**Date:** 2025/04/25
<br>**Bug Fixes:** No additional fixes
* **Known Bug:** when "Properties" is open, pressing "A" sometimes starts typing in the "Name" field

**Additions / Changes:**
* **1)** Bodies are no longer deselected or unfollowing when clicking into space
* **2)** Unfollowing now requires movement keys *(WASD)* to be pressed

---

### Version: 1.8.0
**Date:** 2025/04/24
<br/>**Bug Fixes:** No additional fixes
* **Note:** with the new overhaul, some screens don't render properly yet *(some screens render in a corner)*
* **Note:** mouse detection over UI has been temporarily disabled, so clicks will pass through buttons *(function doesn't work, reworking)*

**Additions / Changes:**
* **1)** Big code refactor across the entire app to consolidate all current features
    * **a)** Separated Simulator UI from Simulation *(a simulation can now exist without UI)*
    * **b)** Generalised simulator UI code to make adding new items easier *(like modes and bodies)*
    * **c)** Separate Physics handler *(physics can be calculated independent of simulations)*
    * **d)** Separate Rendering handler *(visuals, like lines and labels, can be drawn independent of simulations)*
    * **e)** Separate UI Controller to handle common UI processes, like screen switching *(easier to add more screens and compartmentalise)*
* **2)** New time flow button to toggle rewinding, and time label
* **Note:** the time flow button and label are visual only for now

---

### Version: 1.7.2
**Date:** 2025/04/23
<br/>**Bug Fixes:**
* **1)** Fixed labels persisting after leaving a simulation

**Additions / Changes:**
* **1)** Added a button to toggle the visibility of body labels

---

### Version: 1.7.1
**Date:** 2025/04/23
<br/>**Bug Fixes:**
* **1)** Selecting a body for the first time caused a frame skip due to loading the "BodyLabel" prefab *(all prefabs are now preloaded)*
* **Note:** Scrolls over UI are also captured as scrolls in space - this fix is a WIP!

**Additions / Changes:**
* **1)** Updated all current icons with higher resolution ones
* **2)** Added a search bar to the explorer to search for bodies in the current system by name
* **3)** Reduced the thickness of the explorer scroll bar
* **4)** Body previews are labeled as "New Star" / "New Planet" (etc.), then named after their type ("Star", "Planet", etc.)

---

### Version: 1.7.0
**Date:** 2025/04/22
<br/>**Additions / Changes:**
* **1)** New physical name labels for bodies, which can be edited by changing the name of the body

---

### Version: 1.6.4
**Date:** 2025/04/22
<br/>**Bug Fixes:**
* **1)** Clicking on UI elements now blocks the click from passing through *(no more deselection when clicking UI!)*
* **2)** Fixed property window updates preventing field input *(constantly overwrote edits with the current value)*
* **3)** Fixed "No bodies" label not showing in explorer window with no bodies present
* **4)** Fixed explorer entries persisting between simulations
* **5)** Added handling when deleting a body which is currently being followed
* **6)** Fixed explorer and properties titles preventing clicks on their close buttons

**Additions / Changes:**
* **1)** Updated UI for properties window and generalised back-end functions
* **2)** Removed "Speed" property from the properties window
* **3.1)** Changing a body's Mass adjusts its Density
* **3.2)** Changing a body's Radius adjusts its Density
* **3.3)** Changing a body's Density adjusts its Radius
* **4)** Some visual changes to UI for explorer window

---

### Version: 1.6.3
**Date:** 2025/04/22
<br/>**Additions / Changes:**
* **1)** New icons for toolbar and toolbox options

---

### Version: 1.6.2
**Date:** 2025/04/22
<br/>**Additions / Changes:**
* **1)** New button to toggle whether the simulation is running
* **2)** Bodies in "Orbit" mode can only be placed as close to the parent as the distance of their combined radii
* **3)** Allowed bodies to be selected while holding a body preview
* **4)** Selecting a body again deselects it

---

### Version: 1.6.1
**Date:** 2025/04/21
<br/>**Bug Fixes:**
* **1)** Made the underside of the grid plane visible also

**Additions / Changes:**
* **1)** New explorer entry delete button to remove the corresponding body from the system
* **2)** Added keybind to toggle the Explorer with Tab
* **3)** Removed grid plane texture fadeout over Y elevation *(is now constantly "opaque")*

---

### Version: 1.6.0
**Date:** 2025/04/21
<br/>**Bug Fixes:** No additional fixes
* **Note:** Selecting UI doesn't block mouse clicks in space - WIP

**Additions / Changes:**
* **1)** Added a placeable Moon body *(same core functionality as Stars and Planets, different properties)*
* **2)** Generalised code for body creation to cover different classes

---

### Version: 1.5.3
**Date:** 2025/04/21
<br/>**Bug Fixes:**
* **1)** Fixed grid plane interfering with body selection

**Additions / Changes:**
* **1)** Semi-working scaling to adjust camera when zooming towards a point in space
* **2)** Temporarily made grid visual fixed *(will make dynamic later)*
* **Note:** scaling the camera position with the system is a WIP

---

### Version: 1.5.2
**Date:** 2025/04/20
<br/>**Additions / Changes:**
* **1)** Added a button to select a base distance scale *(metres, kilometres, AU, etc.)*
* **2)** Adjusted styling and order for toolbox buttons

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
* **Note:** making "Plane" mode a standalone placement method is a WIP

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
* **1)** Fixed bodies not being followed after selecting the corresponding explorer entry

**Additions / Changes:**
* **1)** Editing a body's details now updates the corresponding entry in the explorer

---

### Version: 1.3.0
**Date:** 2025/04/18
<br/>**Bug Fixes:**
* **1)** Closing the properties window while following a body stops following the body
* **2)** Pressing movement keys (WASD) while a textbox is selected no longer moves the camera

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
* **1)** New Explorer panel *(not yet usable)*
* **2)** New panel to distinguish between Body List and Explorer
* **3)** Bodies can be followed by selecting them *(follows the planet until deselected)*
* **4)** Added a new grid plane with a texture *(currently invisible, WIP)*

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