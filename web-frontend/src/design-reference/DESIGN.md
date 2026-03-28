# Design System Specification

## 1. Overview & Creative North Star: "The Silent Engine"

This design system is built for a SaaS workflow environment that demands high cognitive load management. Our Creative North Star is **"The Silent Engine."** 

Unlike traditional "noisy" SaaS platforms that use aggressive borders and high-contrast separators, this system operates on the principle of **Tonal Architecture**. We break the "template" look by favoring intentional asymmetry and depth over rigid grids. The interface should feel like a high-end physical workspace—sophisticated, layered, and quiet—where the user’s data is the only thing that "pops."

### Design Principles
*   **Atmospheric Depth:** Using tonal shifts rather than lines to define space.
*   **Intentional Friction:** Generous whitespace (`spacing.16` and `spacing.20`) to slow down the eye and focus on critical decision points.
*   **Subtle Luminance:** Using the primary teal (`#1D9E75`) not just as a color, but as a light source.

---

## 2. Colors & Surface Logic

The palette is designed to recede, providing a "Deep Dark" canvas that reduces eye strain during long-form workflow orchestration.

### The "No-Line" Rule
**Explicit Instruction:** Do not use 1px solid borders for sectioning or layout containers. Boundaries must be defined solely through background color shifts. To separate a sidebar from a main content area, place a `surface-container-low` section against a `surface` background. The human eye is sophisticated enough to perceive the change in hex value without a "safety line."

### Surface Hierarchy & Nesting
Treat the UI as a series of physical layers. Use the following tiers to create a "nested" depth model:
*   **Base Layer:** `surface` (#131313) for the main application background.
*   **Secondary Sections:** `surface-container-low` (#1C1B1B) for sidebars or navigation rails.
*   **Information Cards:** `surface-container` (#201F1F) for primary content modules.
*   **Elevated Modals/Popovers:** `surface-container-highest` (#353534) to bring critical actions to the foreground.

### The "Glass & Gradient" Rule
To elevate the experience beyond flat minimalism:
*   **Floating Elements:** Use `surface-variant` at 60% opacity with a `backdrop-blur` of 12px for dropdowns and floating toolbars.
*   **Signature Textures:** For primary CTAs, use a subtle linear gradient from `primary` (#68DBAE) to `primary-container` (#26A37A) at a 135-degree angle. This adds a "jewel-like" quality that flat hex codes lack.

---

## 3. Typography: The Editorial Voice

We utilize a dual-typeface system to balance technical precision with executive authority.

*   **Display & Headlines (Manrope):** Chosen for its geometric stability. Use `display-lg` and `headline-md` with tighter letter-spacing (-0.02em) to create a bold, editorial feel in hero sections or dashboard summaries.
*   **Interface & Body (Inter):** The workhorse. Inter’s high x-height ensures readability at `body-sm` (0.75rem) for complex data tables. 

**Hierarchy Note:** Always prioritize `on-surface-variant` (#BCCAC1) for secondary text and labels. Reserve `on-surface` (#E5E2E1) for primary headers to ensure the most important information is the first thing read.

---

## 4. Elevation & Depth: Tonal Layering

### The Layering Principle
Depth is achieved by "stacking" surface tiers. Place a `surface-container-lowest` card on a `surface-container-low` section to create a soft, natural "recessed" effect. This mimics physical engraving rather than "floating" shadows.

### Ambient Shadows
When a component must float (e.g., a context menu), use a shadow color derived from the `on-surface` token at 4% opacity:
*   **Shadow:** `0px 8px 32px rgba(229, 226, 225, 0.04)`
This creates a "glow" of dark light rather than a muddy grey drop shadow.

### The "Ghost Border" Fallback
If a border is required for accessibility in complex forms, use the `outline-variant` token (#3D4943) at **20% opacity**. 100% opaque borders are strictly forbidden.

---

## 5. Components

### Buttons
*   **Primary:** Filled with the Teal Gradient (Primary to Primary-Container). Roundedness: `md` (0.375rem).
*   **Secondary:** Ghost variant. No background. Border: `outline-variant` at 20%. Text: `on-surface`.
*   **Interaction:** On hover, primary buttons should emit a `primary_fixed` (#86F8C9) outer glow (blur: 15px, spread: -5px).

### Cards & Data Lists
*   **Constraint:** Forbid the use of divider lines between list items.
*   **Separation:** Use `spacing.3` (1rem) vertical gaps and subtle background shifts.
*   **Status Indicators:** Use `primary` (#68DBAE) for "active" and `tertiary` (#FFB3AD) for "paused."

### Input Fields
*   **Canvas:** Use `surface-container-lowest` (#0E0E0E) for the input well to create an "inset" feel.
*   **Focus State:** Transition the `outline` to `primary` (#68DBAE) at 40% opacity. Avoid heavy glow.

### Workflow Nodes (SaaS Specific)
*   Represent workflow steps as `surface-container` cards with an `xl` (0.75rem) corner radius. 
*   Connect nodes with "Ghost Paths"—2px wide paths using `outline-variant` (#3D4943) at 15% opacity.

---

## 6. Do’s and Don'ts

### Do
*   **Do** use asymmetrical spacing. A wider left margin (`spacing.12`) than right margin can create an editorial, high-end look.
*   **Do** use `surface-bright` (#3A3939) for hover states on dark cards to create "lighting" effects.
*   **Do** utilize `title-sm` for labels instead of standard small caps to maintain a sophisticated tone.

### Don't
*   **Don't** use pure black (#000000) or pure white (#FFFFFF). Stick to the `surface` (#131313) and `on-surface` (#E5E2E1) tokens to maintain the "calm" professional tone.
*   **Don't** use icons as the primary method of communication. Pair them with `label-md` typography to ensure the system feels "confident" and "efficient."
*   **Don't** use standard "Modal" overlays that dim the background to black. Use a `backdrop-blur` of 20px to maintain the "Glassmorphism" depth.