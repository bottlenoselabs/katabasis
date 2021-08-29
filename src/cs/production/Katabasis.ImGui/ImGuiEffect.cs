using System.Reflection;

namespace Katabasis.ImGui
{
    public class ImGuiEffect : Effect
    {
        public ImGuiEffect() : base(
            FromStream(Assembly.GetAssembly(typeof(ImGuiEffect))!
                .GetManifestResourceStream("Katabasis.ImGui.Main.fxb")!))
        {
        }
    }
}