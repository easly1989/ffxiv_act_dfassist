using System.Drawing;

namespace DFAssist
{
    public static class OverlayHandler
    {
        public static void ShowTest()
        {
            var charm = new Charm();
            charm.CharmSetOptions(Charm.CharmSettings.CHARM_DRAW_FPS | Charm.CharmSettings.CHARM_REQUIRE_FOREGROUND);
            charm.CharmInit(RenderLoop, "notepad++");
        }

        private static void RenderLoop(Charm.RPM rpm, Charm.Renderer renderer, int width, int height)
        {
            renderer.DrawLine(0, 0, width, height, 5, Color.Magenta);
        }
    }
}
