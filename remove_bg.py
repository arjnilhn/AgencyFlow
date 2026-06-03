from PIL import Image

def remove_background(input_path, output_path):
    img = Image.open(input_path).convert("RGBA")
    width, height = img.size
    pixels = img.load()

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            # Calculate brightness
            brightness = (r + g + b) / 3
            if brightness < 40:
                # Soft blend to transparent
                alpha = int(max(0, (brightness - 15) * (255 / 25)))
                pixels[x, y] = (r, g, b, alpha)

    img.save(output_path, "PNG")

try:
    remove_background(r"C:\Users\Hp\OneDrive\Masaüstü\AgencyFlow\wwwroot\images\agencyflow_logo.png", r"C:\Users\Hp\OneDrive\Masaüstü\AgencyFlow\wwwroot\images\agencyflow_logo_transparent.png")
    print("Success")
except Exception as e:
    print(f"Error: {e}")
