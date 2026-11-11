# Localization update
## and some minor things.

### What I did

- Added translations to es_419 (Latam Spanish) to all the pre-existing strings in English.
- Created new strings for most of the hardcoded texts.
  - Option Groups
  - Button Names
- Reordered the Modifiers list in the settings to match both chance and ammount in the same place. (Some modifiers had their settings scattered)

### My recommendations

- Don't place text in any button. The text should be generated with the locale files. Hardcodding unnecessary leads to double text or unreadable messes.

- Use MiraLocale for texts in the future, as it saves time for localizing the mod, or at least report the lack of strings so it can be worked on by me.

- Avoid walls of text in general. It gets messy in-game and some languages (like Spanish) get even longer. 

- For localizing to other languages I recommend: Copy the en_us.xml and translate from there; and read my guide for translating / writing:

#### TEXT GUIDELINES (to avoid overflow and walls of text, and for localization purposes)

  - Intro Blurbs should be less than 42 characters (it overflows outside the screen).

  - Avoid using walls of text in MedDescription, this is used in the Role Settings and the wiki preview.

  - DO use [nl] or line breaks in TabDescription or Tab texts, because the tab will cover the WHOLE SCREEN if you don't.

  - WikiDescription is the text seen when you click a role icon in the new Mira API wiki.

  - When creating (or translating) settings or setting groups, avoid using long sentences, bc the text shrinks to use up the space and becomes unreadable.

  - PLEASE keep everything that's new in place or add it to the end of the document in the "Other" section so it can be moved later.