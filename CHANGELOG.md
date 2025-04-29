## Changelog

This changelog covers a list of features and fixes which have been made for the application.

Versions are in the form **X.Y.Z**: 
*X* represents a significant version covering a range of unique functionalities; 
*Y* represents single feature or set of related additions;
and *Z* represents fixes, or build-ups to the same feature as a gradual release.

All dates are in the form **YYYY/MM/DD**.

---

### Version: 1.15.1
**Date:** 2025/04/29
<br/>**Additions / Changes:**
* **1)** New to-scale Sun, Earth, and Moon templates with textures *(built-in)*
* **2)** Original Star, Planet, and Moon bodies have been scaled up to fit the new scale
* **3)** "G" is now based on the real value *(so 1G is approx. 6e-11, 2G is double that, etc.)*
* **4)** Added notices for "G" where relevant that the value provided is relative
* **5)** Added "Earth Radius" and "Sun Radius" as distance scales
* **6)** Added an orbit mode label showing the scaled distance to the target body, if any
* **7)** Reverted scale unit capitalisation *(conflicts with AU, so maybe not)*
* **8)** Added selection highlight to "Explorer", "Bodies", and "Time" toolbox buttons *(missed out)*
* **9)** Stars now have a basic pointlight *(no brightness scaling yet)*

---

### Version: 1.15.0
**Date:** 2025/04/29
<br/>**Additions / Changes:**
* **1)** Added angular velocity field to all body types
* **2)** Generalised body copy methods to also cover any added fields
* **3)** Added a "Rotations" field to the properties window *(revolutions per unit of time)*
* **4)** Added scaling indicators to some fields in the properties window *(e.g., "Radius" is relative to the current distance "d")*
* **5)** Made property fields less wide to give more space to field labels
* **6)** Adjusted the size of the explorer search bar

---

### Version: 1.14.0
**Date:** 2025/04/28
<br/>**Bug Fixes:**
* **1)** Verified orbit path rendering for different inclinations *(not sure if it was even broken for this version...)*

**Additions / Changes:**
* **1)** General code cleanup for Simulator Controller / Handler
* **2)** Restructured toolbox buttons
    * **a)** Made "Plane Mode" a standalone mode *(can be used in conjunction with other modes)*
    * **b)** Added a new set of plane tools to manipulate the plane *(raise, lower, rotate)* - use the magnet tool to toggle grid stickiness
    * **c)** Created a separate list for time-related buttons
    * **d)** Removed "Mode" list and moved its icons to the main tool box
* **3)** Reworked placement modes
    * **a)** Removed "Free Placement" as an explicit mode - placement will be free by default, but can be placed on the plane with the new stick button
    * **b)** Separated modes into separate variables to allow multiple modes to be used at once *(e.g., plane and orbit mode)*
* **4)** Added a "Fixate Plane" button to body tools to align the plane with the selected body *(currently visual only)*
* **5)** Used "Help Popup" to the "Tutorial" section instead
* **6)** Replaced icon "Disabled" slash with selection highlighting instead
* **7)** Added a "Toggle Background" button to switch between a space and blank black background

- **Note:** for the Main Menu, the tutorial popup doesn't render properly yet!

---

### Version: 1.13.1
**Date:** 2025/04/28
<br/>**Bug Fixes:**
* **1)** Reverted orbit path tail transparency *(wrong values changed, decided to leave it)*
* **2)** Fixed orbit direction not being used in calculations at all *(and amended how they affect velocity)*
* **3)** Fixed orbit direction keybind being a "hold down" control *(now only single press)*
* **4)** Amended orbit calculation to accounting for parent body's velocity
* **5)** Fixed simulation not pausing when using "Save As"

**Additions / Changes:**
* **1)** Added an eccentricity label in the placement mode's list *(visible when in orbit mode)*
* **2)** Made scale label unit lowercase *(for style)*

---

### Version: 1.13.0
**Date:** 2025/04/28
<br/>**Additions / Changes:**
* **1)** Orbit paths now also render relative to orientation
* **2)** Orbits can now be made elliptical by adjusting their eccentricity
* **3)** Added keybinds to adjust orbit eccentricity *("Z" to decrease, "X" to increase)*
* **4)** Added keybind to swap the direction of orbit *("C" to swap)*
* **5)** Made orbit paths slightly less transparent to make tail end more visible
* **6)** Added finer time intervals *(15s, 30s, 45s, etc.)*

---

### Version: 1.12.1
**Date:** 2025/04/27
<br/>**Additions / Changes:**
* **1)** Added ready-to-use "time coefficient" value to physics renderer
* **2)** Added acceleration magnitude to properties window
* **3)** Time-sensitive / scale-sensitive properties now scale with the current time / scale in the properties window

---

### Version: 1.12.0
**Date:** 2025/04/27
<br/>**Additions / Changes:**
* **1)** Added speed & slow time tool box buttons
* **2)** Time can be sped or slowed *(only a basic timestep multiplier!)* 
* **3)** "Time" is now relative to real seconds rather than frames *(using delta time)*
* **4)** Restored subgrid to the plane visual *(1/10th of the larger grid)*

---

### Version: 1.11.3
**Date:** 2025/04/27
<br/>**Additions / Changes:**
* **1)** "Template Simulations" is now functional - template systems can be viewed and opened
* **2)** Corrected Template Simulations screen alignment *(centralised)*
* **3)** Adjusted styling of saving-related UIs to be consistent *(styles were from former versions)*

---

### Version: 1.11.2
**Date:** 2025/04/27
<br/>**Bug Fixes:**
* **1)** Amended "Template" body item functionality *(was considered an actual body type)*

**Additions / Changes:**
* **1)** New body templates panel to view and select saved body templates in a simulation
* **2)** "Template" body button is now functional - template bodies can be selected and managed
* **3)** Renamed instances of "Body Templates Popup" to "Save Body Popup" (and similar occurrences) to distinguish from new "Body Templates" popup

---

### Version: 1.11.1
**Date:** 2025/04/27
<br/>**Additions / Changes:**
* **1)** Reorganised popup code *(made popups independent of simulator UI)*
* **2)** New "Template" button added to body list *(currently visual only)*

---

### Version: 1.11.0
**Date:** 2025/04/27
<br/>**Bug Fixes:**
* **1)** Fixed trails missing for bodies loaded from saved simulations
* **2)** Fixed trails persisting to main menu sample when leaving a simulation *(included in simulation disable)*
* **3)** Replaced default save entry description with the save's real description
* **4)** Fixed issue where using "Save As" wouldn't switch to the new copy of the simulation *(continued to use the old ID)*
* **5)** Fixed "Help" close button not closing the popup when clicked *(missing functionality)*
* **6)** Fixed tool bar button tooltips shifting simulator popup windows *(adjusted layout)*

**Additions / Changes:**
* **1)** Data handling code cleanup *(generalised methods)*
* **2)** Reorganisation of UI names / styles *(backend)*
* **3)** Bodies can now be saved as templates
* **4)** "Save As Template" body tool is now functional - bodies can be saved as templates with a name and description
* **5)** Clicking a button with a tooltip removes its tooltip *(mainly to stop popup shifting)*
* **Note:** body templates can be saved but not yet used

---

### Version: 1.10.6
**Date:** 2025/04/27
<br/>**Bug Fixes:** no additional fixes
* **Note:** small and large trail streaks are still left when scaling - bug NOT fixed

**Additions / Changes:**
* **1)** Added "Focus" and "Scale" as body tool buttons *(same as "F" and "V" respectively)
* **2)** Added "Save As Template" to body tools *(currently visual only)*

---

### Version: 1.10.5
**Date:** 2025/04/26
<br/>**Bug Fixes:** No additional fixes
* **1)** Fixed small trail streaks when adjusting scale
* **Note:** large streaks are left when adjusting the scale - trails are a WIP

**Additions / Changes:**
* **1)** Icon buttons now show tooltips for what they are when hovered over
* **Note:** some tooltips appear offscreen and cause popups to shift - tooltips are a WIP

---

### Version: 1.10.4
**Date:** 2025/04/26
<br/>**Bug Fixes:**
* **1)** Fixed "Options" popup styles *(used same stylesheet as "Help" popup)*

**Additions / Changes:**
* **1)** Added trails to bodies to show where they've passed in space
* **2)** Renamed "Trajectory" to "Trail" *(makes more sense since it's the path behind)*
* **3)** Swapped label and trail buttons in body tools to be consistent with toolbox order
* **4)** "Trail" *("Trajectory")* body tool is now functional - all / individual body trails can be toggled
* **5)** Added global trail button in toolbox to toggle all trails
* **Note:** for now, trails are completely cleared when scaling or toggling trails *(trail texture "jumps" in both cases)* - this is a WIP

---

### Version: 1.10.3
**Date:** 2025/04/26
<br/>**Additions / Changes:**
* **1)** New class to generalise and hold simulator controls *(for possible customisation in the future)*
* **2)** "Help" button is now functional - displays how to use the simulator and its controls
* **3)** "Save As" and "Options" popups can be toggled with the buttons themselves

---

### Version: 1.10.2
**Date:** 2025/04/26
<br/>**Additions / Changes:**
* **1)** "Anchor" body tool is now functional - bodies can be made unaffected by gravity and locked in place

---

### Version: 1.10.1
**Date:** 2025/04/26
<br/>**Additions / Changes:**
* **1)** "Label" body tool is now functional - individual body labels can toggled
* **2)** Adjusted star brightness *(visually more luminous)*

---

### Version: 1.10.0
**Date:** 2025/04/26
<br/>**Bug Fixes:**
* **1)** Fixed visual bug where play button change to paused state when opening options *(no visual change now)*

**Additions / Changes:**
* **1)** Added "Help" button to simulator toolbar *(currently visual only, will display controls, etc.)*
* **2)** Added a toolbar for body tools to manage the current selected body:
    * **a)** **Properties:** toggles the properties window containing the body's details
    * **b)** **Trajectory:** toggle the path the body has taken through space *(currenlty visual only)*
    * **c)** **Label:** toggle the body's label *(currently visual only)*
    * **d)** **Anchor:** toggle whether the body is affected by gravity *(currently visual only)*
    * **e)** **Delete:** remove the body from the system
* **3)** Adjusted icons for label buttons

---

### Version: 1.9.4
**Date:** 2025/04/26
<br/>**Bug Fixes:**
* **1)** Fixed movement controls not being disabled when typing in fields *(misassigned event)*
* **2)** Fixed properties window briefly showing fields when opened for the first time with no selection

**Additions / Changes:**
* **1)** Restored speed field to properties window *(read only)*
* **2)** Simulator keybind operations are no longer when typing in fields

---

### Version: 1.9.3
**Date:** 2025/04/26
<br/>**Additions / Changes:**
* **1)** Gravitational constant is now given in terms of "G" *("G" is still "1" for now)*
* **2)** New options popup to change the settings and information of the current simulation
* **3)** Restored Collision Quality field in options *(currently visual only)*

---

### Version: 1.9.2
**Date:** 2025/04/26
<br/>**Bug Fixes:**
* **1)** Fixed issue where pressing movement keys *(WASD)* would focus on fields if the properties window is open

---

### Version: 1.9.1
**Date:** 2025/04/26
<br/>**Bug Fixes:**
* **1)** Fixed simulation not being interactable at all when closing the "Save As" popup
* **2)** Fixed followed body's "ghost" persisting between simulations *(creating a new body in a new simulation in orbit mode would show a path around this ghost)*
* **3)** Amended code for save deletion *(would fail part way through)*
* **4)** Fixed highlighting and padding when hovering over save entries

**Additions / Changes:**
* **1)** New save button to save a simulation directly
* **2)** Saving a new simulation for the first time prompts to "Save As" a new simulation
* **3)** New options button to access simulation settings and details *(currently visual only)*
* **4)** Saved simulations are paused when opened

---

### Version: 1.9.0
**Date:** 2025/04/25
<br/>**Bug Fixes:**
* **1)** Fixed simulator title not updating to the opened simulation's name
* **2)** Fixed scale label not updating when adjusting the scale to fit the followed body
* **3)** Fixed scale label not updating when selecting a distance metric
* **4)** Fixed distance metric selection not resetting the zoom factor to 0
* **5)** Fixed main menu camera being "zoomable" after returning to it from a simulation

**Additions / Changes:**
* **1)** New popup to save the current simulation as a new save or template
* **2)** Saved systems can now be loaded *(initially paused)*
* **3)** Removed "Gravity Algorithm" and "Collision Quality" fields from New Simulation interface *(will be separate app options)*
* **4)** Adjustment to UI for New Simulation window
* **5)** Decreased near clip range to view smaller scale bodies closer
* **Note:** currently, only the "New Save" option works when saving the current system

---

### Version: 1.8.4
**Date:** 2025/04/25
<br/>**Additions / Changes:**
* **1)** Added an auto-generated sample system to the main menu *(to test auto-generation)*

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
* **3)** New "V" keybind to view a followed body at an appropriate scale

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