using Engine;
using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX {
    /// <summary>
    /// 工作台 / 熔炉合成提示展示；<see cref="IsEnabled"/> 可由内容模组挂接（如 IESettings）。
    /// </summary>
    public static class CrafterHints {
        public static System.Func<bool> IsEnabled { get; set; } = () => true;

        public static void TryShow(ComponentPlayer? player, FormattedRecipe? recipe) {
            if (!IsEnabled() || player == null || recipe == null || string.IsNullOrEmpty(recipe.Message)) return;
            string message = recipe.Message;
            if (message.StartsWith('[') && message.EndsWith(']')) {
                message = LanguageControl.Get("CRMessage", message.Substring(1, message.Length - 2));
            }
            player.ComponentGui.DisplaySmallMessage(message, Color.White, blinking: true, playNotificationSound: true);
        }
    }
}
