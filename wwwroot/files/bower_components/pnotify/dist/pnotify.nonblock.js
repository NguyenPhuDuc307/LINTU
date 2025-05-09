(function (h, f) {
  "function" === typeof define && define.amd
    ? define("pnotify.nonblock", ["jquery", "pnotify"], f)
    : "object" === typeof exports && "undefined" !== typeof module
    ? (module.exports = f(require("jquery"), require("./pnotify")))
    : f(h.jQuery, h.PNotify);
})(this, function (h, f) {
  var l = /^on/,
    m =
      /^(dbl)?click$|^mouse(move|down|up|over|out|enter|leave)$|^contextmenu$/,
    n = /^(focus|blur|select|change|reset)$|^key(press|down|up)$/,
    p = /^(scroll|resize|(un)?load|abort|error)$/,
    k = function (c, b) {
      var d;
      c = c.toLowerCase();
      document.createEvent && this.dispatchEvent
        ? ((c = c.replace(l, "")),
          c.match(m)
            ? (h(this).offset(),
              (d = document.createEvent("MouseEvents")),
              d.initMouseEvent(
                c,
                b.bubbles,
                b.cancelable,
                b.view,
                b.detail,
                b.screenX,
                b.screenY,
                b.clientX,
                b.clientY,
                b.ctrlKey,
                b.altKey,
                b.shiftKey,
                b.metaKey,
                b.button,
                b.relatedTarget
              ))
            : c.match(n)
            ? ((d = document.createEvent("UIEvents")),
              d.initUIEvent(c, b.bubbles, b.cancelable, b.view, b.detail))
            : c.match(p) &&
              ((d = document.createEvent("HTMLEvents")),
              d.initEvent(c, b.bubbles, b.cancelable)),
          d && this.dispatchEvent(d))
        : (c.match(l) || (c = "on" + c),
          (d = document.createEventObject(b)),
          this.fireEvent(c, d));
    },
    g,
    e = function (c, b, d) {
      c.elem.addClass("ui-pnotify-nonblock-hide");
      var a = document.elementFromPoint(b.clientX, b.clientY);
      c.elem.removeClass("ui-pnotify-nonblock-hide");
      var f = h(a),
        e = f.css("cursor");
      "auto" === e && "A" === a.tagName && (e = "pointer");
      c.elem.css("cursor", "auto" !== e ? e : "default");
      (g && g.get(0) == a) ||
        (g &&
          (k.call(g.get(0), "mouseleave", b.originalEvent),
          k.call(g.get(0), "mouseout", b.originalEvent)),
        k.call(a, "mouseenter", b.originalEvent),
        k.call(a, "mouseover", b.originalEvent));
      k.call(a, d, b.originalEvent);
      g = f;
    };
  f.prototype.options.nonblock = { nonblock: !1 };
  f.prototype.modules.nonblock = {
    init: function (c, b) {
      var d = this;
      c.elem.on({
        mouseenter: function (a) {
          d.options.nonblock && a.stopPropagation();
          d.options.nonblock && c.elem.addClass("ui-pnotify-nonblock-fade");
        },
        mouseleave: function (a) {
          d.options.nonblock && a.stopPropagation();
          g = null;
          c.elem.css("cursor", "auto");
          d.options.nonblock &&
            "out" !== c.animating &&
            c.elem.removeClass("ui-pnotify-nonblock-fade");
        },
        mouseover: function (a) {
          d.options.nonblock && a.stopPropagation();
        },
        mouseout: function (a) {
          d.options.nonblock && a.stopPropagation();
        },
        mousemove: function (a) {
          d.options.nonblock && (a.stopPropagation(), e(c, a, "onmousemove"));
        },
        mousedown: function (a) {
          d.options.nonblock &&
            (a.stopPropagation(), a.preventDefault(), e(c, a, "onmousedown"));
        },
        mouseup: function (a) {
          d.options.nonblock &&
            (a.stopPropagation(), a.preventDefault(), e(c, a, "onmouseup"));
        },
        click: function (a) {
          d.options.nonblock && (a.stopPropagation(), e(c, a, "onclick"));
        },
        dblclick: function (a) {
          d.options.nonblock && (a.stopPropagation(), e(c, a, "ondblclick"));
        },
      });
    },
  };
});
