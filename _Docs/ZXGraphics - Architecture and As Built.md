# ZXGraphics - Architecture and As Built
ZXGraphics is a fork for the integration of the ZXGraphics Visual Studio Code plugin into ZXBasicStudio.
It is not only an integration, it is more a continuation and evolution of ZXGraphics.

## Architecture
There are four ZXGraphics projects and a "Common" tools proyect.

### ZXGraphics.ui
Implements the user interface part.

### ZXGraphics.log
ZXGraphics logic and service layer.

### ZXGraphics.dat
Data access layer.

### ZXGraphics.neg
It defines the business objects.

### Common
Common tools for all ZXBasicStudio, like .tap utilities, for example.

