Thank you for choosing to import "Procedural Animation in Video Game Characters Unity Package JoshuaC" package
// HUMANOID CONTROLLER (THIRD PERSON CHARACTER CONTROLLER)
The humanoid controller folder contains key humanoid assets, such as a modular Third Person Character controller designed for the setup shown
A second script, "Generic Start Jump" is included to be attatched to the same part of the model as the animator and is triggered during an animation event as to trigger the jump of the character at the correct time of the animation.
Furthermore, the crouching system of the character relies on a second layer named "Crouch Layer" which procedurally crouches the character by applying the adjustments of the layer.
Feel free to take and use the character controller included!

// PROCEDURAL ADJUSTMENTS
In order for the 'Procedural Touch Up' section of the Package to work, the Humanoid folder must also be imported for the demo scene to work as intended, as this contains key assets for the humanoid characters found there
The Procedural Adjustments scripts will simply work by attaching the IK scripts to the same asset as the animator, as they utilise functions of the animator such as the IK PASS checkbox of a layer
Each script can be independently disabled at a system level through the IK Active function, which in the case of the arms and heads will adjust them back to the original position as to not snap back and forth 
Inside the IK Manager script, the Q button is used to toggle procedural animations. Feel free to remove this as this is for demonstration purposes only

// RAGDOLL
In order for the 'Ragdoll' section of the Package to work, the Humanoid folder must also be imported for the demo scene to work as intended, as this contains key assets for the humanoid characters found there
For the ragdoll script, the Unity Ragdoll system must first be applied to the character. This can be accessed through the right click context menu under the 3D section, and will require the transforms of the humanoid characters to be applied to that menu
The ragdoll system can be applied through a single script, although it is currently triggered using the Q key. Feel free to remove this as this is for demonstration purposes only

// SPIDER-BOT
The Spider-Bot folder utilises the Unity Animation Rigging package to work
The demonstration scene utilises a Spider-bot with only 4 legs, however the system is set up in a modular way to allow for an arachnid with any number of legs
Each leg must have a "two-bone ik constraint" component added to them, alongside the targets being added to the "Leg Targets" array

// MATERIALS
The _Materials folder contains all materials used across the 3 demo scenes, however, is not strictly necessary for the code to work

// HEADS UP DISPLAY
Finally, the Script UI_Controller is necessary for functionality across the demo scenes the keyboard outside of movement to work in the prototype, so that must also be imported.

Once again, thank you for importing this package
