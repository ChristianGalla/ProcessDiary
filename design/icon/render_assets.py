"""Renders ProcessDiary-icon.svg into the MSIX assets and the tray icons.

Run with the GIMP 3 console, see Generate-Assets.ps1.
"""
import os
import struct
import tempfile

import gi

gi.require_version("Gimp", "3.0")
from gi.repository import Gimp, Gio  # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__)) if "__file__" in globals() else os.environ["PD_ICON_DIR"]
HERE = os.environ.get("PD_ICON_DIR", HERE)
SVG = os.path.join(HERE, "ProcessDiary-icon.svg")
ASSETS = os.path.join(os.path.dirname(os.path.dirname(HERE)), "ProcessDiary", "Assets")
TRAY = os.path.join(ASSETS, "Tray")
TMP = tempfile.mkdtemp(prefix="pd-icon-")

# (file name, canvas width, canvas height, icon size)
ASSET_FILES = [
	("Square44x44Logo.scale-200.png", 88, 88, 88),
	("Square44x44Logo.targetsize-24_altform-unplated.png", 24, 24, 24),
	("Square150x150Logo.scale-200.png", 300, 300, 300),
	("Wide310x150Logo.scale-200.png", 620, 300, 280),
	("SplashScreen.scale-200.png", 1240, 600, 440),
	("LockScreenLogo.scale-200.png", 48, 48, 48),
	("StoreLogo.png", 50, 50, 50),
]

# Same colors as StatusPresentation.
TRAY_STATES = {
	"Recording": "#D13438",
	"Paused": "#F7B500",
	"Disabled": "#808080",
	"Error": "#8E0000",
}
TRAY_SIZES = [16, 24, 32, 48, 64, 256]


def load_svg(path, width, height):
	proc = Gimp.get_pdb().lookup_procedure("file-svg-load")
	config = proc.create_config()
	config.set_property("run-mode", Gimp.RunMode.NONINTERACTIVE)
	config.set_property("file", Gio.File.new_for_path(path))
	config.set_property("width", width)
	config.set_property("height", height)
	result = proc.run(config)
	return result.index(1)


def save_png(image, path):
	Gimp.file_save(Gimp.RunMode.NONINTERACTIVE, image, Gio.File.new_for_path(path), None)
	image.delete()


def render(svg, out, canvas_w, canvas_h, icon):
	image = load_svg(svg, icon, icon)
	if (canvas_w, canvas_h) != (icon, icon):
		layer = image.get_layers()[0]
		offset_x = (canvas_w - icon) // 2
		offset_y = (canvas_h - icon) // 2
		image.resize(canvas_w, canvas_h, offset_x, offset_y)
		layer.resize_to_image_size()
	save_png(image, out)


def write_ico(png_paths, ico_path):
	blobs = []
	for size, path in png_paths:
		with open(path, "rb") as f:
			blobs.append((size, f.read()))
	header = struct.pack("<HHH", 0, 1, len(blobs))
	offset = 6 + 16 * len(blobs)
	entries = b""
	data = b""
	for size, blob in blobs:
		dim = 0 if size >= 256 else size
		entries += struct.pack("<BBBBHHII", dim, dim, 0, 0, 1, 32, len(blob), offset + len(data))
		data += blob
	with open(ico_path, "wb") as f:
		f.write(header + entries + data)


def status_svg(color):
	with open(SVG, encoding="utf-8") as f:
		svg = f.read()
	dot = (
		'  <circle cx="190" cy="190" r="56" fill="#FFFFFF"/>\n'
		'  <circle cx="190" cy="190" r="42" fill="%s"/>\n</svg>' % color
	)
	return svg.replace("</svg>", dot)


def main():
	os.makedirs(TRAY, exist_ok=True)

	for name, w, h, icon in ASSET_FILES:
		render(SVG, os.path.join(ASSETS, name), w, h, icon)

	app_pngs = []
	for size in TRAY_SIZES:
		png = os.path.join(TMP, "App-%d.png" % size)
		render(SVG, png, size, size, size)
		app_pngs.append((size, png))
	write_ico(app_pngs, os.path.join(ASSETS, "ProcessDiary.ico"))

	for state, color in TRAY_STATES.items():
		svg_path = os.path.join(TMP, "tray-%s.svg" % state)
		with open(svg_path, "w", encoding="utf-8") as f:
			f.write(status_svg(color))
		pngs = []
		for size in TRAY_SIZES:
			png = os.path.join(TMP, "%s-%d.png" % (state, size))
			render(svg_path, png, size, size, size)
			pngs.append((size, png))
		write_ico(pngs, os.path.join(TRAY, "ProcessDiary-%s.ico" % state))


main()
