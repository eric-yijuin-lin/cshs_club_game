function getTemplateObject(selector) {
    const template = $(selector);
    if (!template) {
        throw(`cannot find template by selector: "${selector}"`)
    }
    return $($.parseHTML(template.html()))
}