# hello
sdfsdf

CriterionInstance

```plantuml
  Bob -> Alice : hello
  Alice -> Bob : Go Away
  ```

  problem:
  1) criterioncache braucht das videometamodel 
  Im criterionCache werden alle vmms durchgegangen ob die criterioninstances haben die nicht als thumbail auf der platte liegen und entsprechendes
  dummy thumbnail hinzugefügt

  2) videometamodel braucht den criterioncache
  ist momentan nicht modelliert, sollte aber so sein da hier beim einlesen der filenames die criterioninstances erzeugt werden, allerdings neu was nicht sein sollte
  hie sollten die instanzen aus dem criterioncache genommen werden.

  Steps:
  1) Umziehen der funktion der na criterioninstances nicht im criterioncache zu machen sondern im videometamodelcache
  2) criterioncache --> viodeometamodel löschen
  3) im vmmcache beim einlesen der filenames nicht mehr neue instanzen erzeugen sondern die aus dem cache nehmen