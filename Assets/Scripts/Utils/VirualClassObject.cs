using System;
using System.Collections.Generic;

namespace FairyGUI.Utils {

    public class VirualClassObject {
        private Dictionary<string, Action> methods = new Dictionary<string, Action>();
        
        public static VirualClassObject Instance(Action<VirualClassObject> initializer) {
            var obj = new VirualClassObject();
            if (initializer != null) {
                initializer(obj);
            }
            return obj;
        }
        
        public void AddMethod(string method, Action callback) {
            methods[method] = callback;
        }
        
        public bool Call(string method) {
            Action callback;
            if (methods.TryGetValue(method, out callback)) {
                if (callback != null) {
                    callback();
                    return true;
                }
            }
            return false;
        }
    }

}