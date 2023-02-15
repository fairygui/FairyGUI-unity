using UnityEngine;
using FairyGUI;

public class EmitComponent : GComponent
{
    GLoader _symbolLoader;
    GTextField _numberText;
    Transform _owner;

    const float OFFSET_ADDITION = 2.2f;
    static Vector2 JITTER_FACTOR = new Vector2(80, 80);

    public EmitComponent()
    {
        this.touchable = false;

        _symbolLoader = new GLoader();
        _symbolLoader.autoSize = true;
        AddChild(_symbolLoader);

        _numberText = new GTextField();
        _numberText.autoSize = AutoSizeType.Both;

        AddChild(_numberText);
    }

    public void SetHurt(Transform owner, int type, long hurt, bool critical)
    {
        _owner = owner;

        TextFormat tf = _numberText.textFormat;
        if (type == 0)
            tf.font = EmitManager.inst.hurtFont1;
        else
            tf.font = EmitManager.inst.hurtFont2;
        _numberText.textFormat = tf;
        _numberText.text = "-" + hurt;

        if (critical)
            _symbolLoader.url = EmitManager.inst.criticalSign;
        else
            _symbolLoader.url = "";

        UpdateLayout();

        this.alpha = 1;
        UpdatePosition(Vector2.zero);
        Vector2 rnd = Vector2.Scale(UnityEngine.Random.insideUnitCircle, JITTER_FACTOR);
        int toX = (int)rnd.x * 2;
        int toY = (int)rnd.y * 2;

        EmitManager.inst.view.AddChild(this);
        GTween.To(Vector2.zero, new Vector2(toX, toY), 1f).SetTarget(this)
            .OnUpdate((GTweener tweener) => { this.UpdatePosition(tweener.value.vec2); }).OnComplete(this.OnCompleted);
        this.TweenFade(0, 0.5f).SetDelay(0.5f);
    }

    void UpdateLayout()
    {
        this.SetSize(_symbolLoader.width + _numberText.width, Mathf.Max(_symbolLoader.height, _numberText.height));
        _numberText.SetXY(_symbolLoader.width > 0 ? (_symbolLoader.width + 2) : 0,
        (this.height - _numberText.height) / 2);
        _symbolLoader.y = (this.height - _symbolLoader.height) / 2;
    }

    void UpdatePosition(Vector2 pos)
    {
        Vector3 ownerPos = _owner.position;
        ownerPos.y += OFFSET_ADDITION;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(ownerPos);
        screenPos.y = Screen.height - screenPos.y; //convert to Stage coordinates system

        Vector3 pt = GRoot.inst.GlobalToLocal(screenPos);
        this.SetXY(Mathf.RoundToInt(pt.x + pos.x - this.actualWidth / 2), Mathf.RoundToInt(pt.y + pos.y - this.height));
    }

    void OnCompleted()
    {
        _owner = null;
        EmitManager.inst.view.RemoveChild(this);
        EmitManager.inst.ReturnComponent(this);
    }

    public void Cancel()
    {
        _owner = null;
        if (this.parent != null)
        {
            GTween.Kill(this);
            EmitManager.inst.view.RemoveChild(this);
        }
        EmitManager.inst.ReturnComponent(this);
    }
}