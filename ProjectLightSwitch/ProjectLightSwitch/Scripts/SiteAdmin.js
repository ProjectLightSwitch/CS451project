function AddQuestionRow(table, prompt)
{
    var row = $('<tr>');
    row.append($('<td>').text(prompt));
    row.append($('<td>').append($('<input>'));
}