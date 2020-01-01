# JebelMarraViz
Unity project visualizing topography and rainfall in Jebel (mountain) Marra, Sudan

This project was done "hackathon-style" in < 2 days for a workshop I attended. The project targeted Jebel Marra region, but -in theory- could be used with any other regions data.

Datasets used:
- JAXA ALOS World3D, 30m DSM (in geoTIFF format), via https://opentopography.org/.
- NASA/JAXA TRMM rainfall data (average monthely rainfall, in geoTIFF format), via http://www.geog.ucsb.edu/~bodo/TRMM/


Raster preperation was carried out with QGIS. Major preperations steps were:
- Clipping the rasters so they are starting from the same SW corner and with the same width and height (in coverage).
- Exporting the rasters as rendered values (i.e. exported image's pixels show up exactly as they are symbolized in QGIS), this is important as we need to know what the min/max colour ranges for pixels represent (e.g. for included dataset, for rainfall: min = 0 mm/month and max = 1000 mm/month, while for elevation: min = 1000m and max = 3004m).


Controls:
- W, A, S, and D: standard 3D movement relative to camera orientation (forward, backwards, strafe left, strafe right)
- Q and E: rotate camera left or right.
- Space: Fly upwards.
- C: Drop downwards.
- Brackets [ and ]: Move time forwards or backwards (changes season/rainfall)
- Greater and Less < and >: Increase or decrease ground mesh resolution (does not append automatically, needs manually appending)
- O: Append ground mesh resolution (NOTE: when application starts, it does not 3D-fy the ground automatically, O must be pressed to do so).
- P (useless in current implementation): Append precipitation data (done automatically when changing time, so no use for it).
- 1 (Alpha 1, not Numpad 1): Lock/Unlock camera rotation and movement.

