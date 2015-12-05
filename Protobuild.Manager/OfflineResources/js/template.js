window.addTemplate = function(name, values) {
  console.log("requested to spawn template with name '" + name + "'");

  var original = $("[data-template=\"" + name + "\"]");
  if (original.length == 0) {
    console.log("ERROR! Unable to find template with name '" + name + "'");
  }

  var copy = original.clone();
  
  for (var k in values){
    if (target.hasOwnProperty(k)) {
      copy.find("[data-template-value=\"" + k + "\"]").each(function(e) { e.innerText = values[k]; });
    }
  }

  copy.removeAttr("data-template");
  original.parent().append(copy);

  console.log("template spawned and added to parent");

  return copy;
}
